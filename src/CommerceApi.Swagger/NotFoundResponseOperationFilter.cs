using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger;

public class NotFoundResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.MethodInfo.GetParameters();
        var hasGuidParameter = parameters.Any(p => p.ParameterType == typeof(Guid));
        if (hasGuidParameter)
        {
            operation.Responses.Add(StatusCodes.Status404NotFound.ToString(), GetResponse("item not found"));
        }
    }

    private static OpenApiResponse GetResponse(string description)
    {
        var reference = new OpenApiReference { Id = nameof(ProblemDetails), Type = ReferenceType.Schema };
        var schema = new OpenApiSchema { Reference = reference };

        var jsonMediaType = MediaTypeNames.Application.Json;
        var apiMediaType = new OpenApiMediaType { Schema = schema };
        var content = new Dictionary<string, OpenApiMediaType> { [jsonMediaType] = apiMediaType };

        var response = new OpenApiResponse { Content = content, Description = description };
        return response;
    }
}