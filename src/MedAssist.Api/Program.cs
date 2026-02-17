using MedAssist.Api.Middleware;
using MedAssist.Api.Auth;
using MedAssist.Api.Swagger;
using MedAssist.Application;
using MedAssist.Infrastructure;
using MedAssist.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using FluentValidation.AspNetCore;
using System.Text;

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
builder.Services.AddMemoryCache();
builder.Services
    .AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(AuthOptions.SectionName))
    .ValidateDataAnnotations();

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("MiniAppOnly", policy =>
        policy.RequireClaim(AuthClaimTypes.ActorType, AuthActorTypes.MiniApp));

    options.AddPolicy("BotOnly", policy =>
        policy.RequireClaim(AuthClaimTypes.ActorType, AuthActorTypes.BotService));

    options.AddPolicy("MiniAppOrBot", policy =>
        policy.RequireAssertion(context =>
        {
            var actorType = context.User.FindFirst(AuthClaimTypes.ActorType)?.Value;
            return string.Equals(actorType, AuthActorTypes.MiniApp, StringComparison.Ordinal)
                   || string.Equals(actorType, AuthActorTypes.BotService, StringComparison.Ordinal);
        }));
});

builder.Services.AddScoped<ITelegramInitDataValidator, TelegramInitDataValidator>();
builder.Services.AddScoped<IApiKeyGrantValidator, ApiKeyGrantValidator>();
builder.Services.AddScoped<ITokenService, TokenService>();

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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT access token: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DatabaseInitializer.EnsureDatabaseAsync(app.Services, CancellationToken.None);

app.Run();
