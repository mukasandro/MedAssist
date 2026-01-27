using System.Net;
using MedAssist.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MedAssist.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, cannot handle exception.");
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error.")
        };

        var details = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail =  exception.Message ,
            Instance = context.Request.Path
        };

        _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(details);
    }
}
