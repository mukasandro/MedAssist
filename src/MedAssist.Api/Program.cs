using MedAssist.Api.Swagger;
using MedAssist.Application;
using MedAssist.Infrastructure;
using MedAssist.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
{
    // Гарантируем application/json: убираем текстовый форматтер и навешиваем Produces глобально
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
    options.Filters.Add(new ProducesAttribute("application/json"));
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("admin", new OpenApiInfo
    {
        Title = "MedAssist Admin API",
        Version = "v1",
        Description = "Administrative API surface."
    });
    options.SwaggerDoc("bot", new OpenApiInfo
    {
        Title = "MedAssist Bot API",
        Version = "v1",
        Description = "Bot-facing API surface."
    });
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return false;
        }

        var groups = actionDescriptor.MethodInfo
            .GetCustomAttributes(true)
            .OfType<SwaggerGroupAttribute>()
            .Concat(actionDescriptor.ControllerTypeInfo.GetCustomAttributes(true).OfType<SwaggerGroupAttribute>())
            .Select(a => a.GroupName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return groups.Contains(docName);
    });
    options.ExampleFilters();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/bot/swagger.json", "MedAssist Bot API");
        options.SwaggerEndpoint("/swagger/admin/swagger.json", "MedAssist Admin API");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(app.Environment.IsDevelopment() ? "DevCors" : "DefaultCors");
app.UseAuthorization();
app.MapControllers();

await DatabaseInitializer.EnsureDatabaseAsync(app.Services, CancellationToken.None);

app.Run();
