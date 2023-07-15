using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.ClientContext;

internal class ClientContextOperationFilter : IOperationFilter
{
    private readonly ClientContextOptions _options;

    public ClientContextOperationFilter(ClientContextOptions options)
    {
        _options = options;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _options.TimeZoneHeader,
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
            }
        });
    }
}