using System;

namespace MedAssist.Api.Swagger;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class SwaggerGroupAttribute : Attribute
{
    public SwaggerGroupAttribute(string groupName)
    {
        GroupName = groupName;
    }

    public string GroupName { get; }
}
