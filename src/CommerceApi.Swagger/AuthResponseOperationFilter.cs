using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger;

public class AuthResponseOperationFilter : IOperationFilter
{
    private readonly IAuthorizationPolicyProvider _policyProvider;

    public AuthResponseOperationFilter(IAuthorizationPolicyProvider policyProvider)
    {
        _policyProvider = policyProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fallbackPolicy = _policyProvider.GetFallbackPolicyAsync().GetAwaiter().GetResult();
        var requireAuthenticatedUser = fallbackPolicy?.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement) ?? false;

        var requireAuthorization = context.MethodInfo
            .DeclaringType?
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(a => a is AuthorizeAttribute) ?? false;

        var allowAnonymous = context.MethodInfo
            .DeclaringType?
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(a => a is AllowAnonymousAttribute) ?? false;

        if ((requireAuthorization || requireAuthenticatedUser) && !allowAnonymous)
        {
            var unauthorizedStatusCode = StatusCodes.Status401Unauthorized.ToString();
            var forbiddenStatusCode = StatusCodes.Status403Forbidden.ToString();

            var unauthorizedResponse = GetResponse(HttpStatusCode.Unauthorized.ToString());
            var forbiddenResponse = GetResponse(HttpStatusCode.Forbidden.ToString());

            operation.Responses.TryAdd(unauthorizedStatusCode, unauthorizedResponse);
            operation.Responses.TryAdd(forbiddenStatusCode, forbiddenResponse);
        }
    }

    private static OpenApiResponse GetResponse(string description)
    {
        var reference = new OpenApiReference
        {
            Id = nameof(ProblemDetails),
            Type = ReferenceType.Schema
        };

        var schema = new OpenApiSchema { Reference = reference };
        var mediaType = new OpenApiMediaType { Schema = schema };

        var jsonMediaType = MediaTypeNames.Application.Json;
        var content = new Dictionary<string, OpenApiMediaType> { [jsonMediaType] = mediaType };

        var response = new OpenApiResponse { Description = description, Content = content };
        return response;
    }
}