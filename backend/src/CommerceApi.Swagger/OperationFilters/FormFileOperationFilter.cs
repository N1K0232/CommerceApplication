using CommerceApi.Swagger.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger.OperationFilters;

internal class FormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileContent = typeof(FormFileContent);
        var parameters = context.MethodInfo.GetParameters();
        var acceptsFormFile = parameters.FirstOrDefault(p => p.ParameterType == fileContent);

        if (acceptsFormFile != null)
        {
            var fileSchema = new OpenApiSchema { Type = "string", Format = "binary" };
            var titleSchema = new OpenApiSchema { Type = "string", Required = new HashSet<string> { "title" } };
            var descriptionSchema = new OpenApiSchema { Type = "string" };

            var properties = new Dictionary<string, OpenApiSchema>
            {
                { "file", fileSchema},
                { "title", titleSchema},
                { "description", descriptionSchema }
            };

            var formSchema = new OpenApiSchema
            {
                Type = "object",
                Required = new HashSet<string> { "file" },
                Properties = properties
            };

            var formMediaType = new OpenApiMediaType { Schema = formSchema };
            var content = new Dictionary<string, OpenApiMediaType>
            {
                { "multipart/form-data", formMediaType }
            };

            var requestBody = new OpenApiRequestBody { Content = content };
            operation.RequestBody = requestBody;
        }
    }
}