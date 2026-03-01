using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MedAssist.LlmGateway.Api.Swagger;

public class GenerateRequestExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var path = context.ApiDescription.RelativePath?.Trim('/');
        if (!string.Equals(path, "v1/generate", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (operation.RequestBody?.Content is null)
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var jsonContent))
        {
            return;
        }

        jsonContent.Example = new OpenApiObject
        {
            ["model"] = new OpenApiString("deepseek-chat"),
            ["temperature"] = new OpenApiDouble(0.2),
            ["maxTokens"] = new OpenApiInteger(1200),
            ["messages"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["role"] = new OpenApiString("system"),
                    ["content"] = new OpenApiString("Ты медицинский ассистент для врача. Отвечай кратко и структурированно.")
                },
                new OpenApiObject
                {
                    ["role"] = new OpenApiString("user"),
                    ["content"] = new OpenApiString("Пациент 54 года, давление 160/100, что уточнить первым делом?")
                }
            }
        };
    }
}
