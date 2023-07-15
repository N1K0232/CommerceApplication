﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger.Abstractions;

public abstract class OperationFilter : IOperationFilter
{
    public abstract void Apply(OpenApiOperation operation, OperationFilterContext context);

    internal virtual OpenApiResponse GetResponse(string description)
    {
        var response = new OpenApiResponse { Description = description };
        return response;
    }
}