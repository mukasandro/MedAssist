using MedAssist.LlmGateway.Api.Options;
using MedAssist.LlmGateway.Api.Providers;
using MedAssist.LlmGateway.Api.Routing;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MedAssist LLM Gateway API",
        Version = "v1"
    });
});

builder.Services
    .AddOptions<LlmGatewayOptions>()
    .Bind(builder.Configuration.GetSection(LlmGatewayOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.DeepSeek.ApiKey),
        "LlmGateway:DeepSeek:ApiKey must be provided via environment variables.")
    .ValidateOnStart();

builder.Services.AddHttpClient<DeepSeekProvider>();
builder.Services.AddScoped<DeepSeekProvider>();
builder.Services.AddScoped<ILLMRouter, LlmRouter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
