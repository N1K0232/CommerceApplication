using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger;

internal class DefaultResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var defaultResponse = GetResponse("Unexpected error");
        operation.Responses.TryAdd("default", defaultResponse);
    }

    private static OpenApiResponse GetResponse(string description)
    {
        var reference = new OpenApiReference { Id = nameof(ProblemDetails), Type = ReferenceType.Schema };
        var schema = new OpenApiSchema { Reference = reference };

        var jsonMediaType = MediaTypeNames.Application.Json;
        var apiMediaType = new OpenApiMediaType { Schema = schema };
        var content = new Dictionary<string, OpenApiMediaType> { [jsonMediaType] = apiMediaType };

        var response = new OpenApiResponse { Description = description, Content = content };
        return response;
    }
}