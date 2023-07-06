using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger.Abstractions;

public abstract class DocumentFilter : IDocumentFilter
{
    public abstract void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context);
}