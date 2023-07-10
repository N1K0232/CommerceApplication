using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.Swagger.OperationFilters;

internal class CultureAwareOperationFilter : IOperationFilter
{
    private readonly RequestLocalizationOptions _localizationOptions;

    public CultureAwareOperationFilter(IOptions<RequestLocalizationOptions> localizationOptions)
    {
        _localizationOptions = localizationOptions.Value;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var supportedLanguages = GetSupportedLanguages();
        if (supportedLanguages?.Count > 1)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var schema = new OpenApiSchema
            {
                Type = "String",
                Enum = supportedLanguages,
                Default = supportedLanguages.First()
            };

            var parameter = new OpenApiParameter
            {
                Name = HeaderNames.AcceptLanguage,
                In = ParameterLocation.Header,
                Required = false,
                Schema = schema
            };

            operation.Parameters.Add(parameter);
        }
    }

    private IList<IOpenApiAny>? GetSupportedLanguages()
    {
        var supportedCultures = _localizationOptions.SupportedCultures?.Select(c => new OpenApiString(c.TwoLetterISOLanguageName));
        var supportedLanguages = supportedCultures?.Cast<IOpenApiAny>().ToList();
        return supportedLanguages;
    }
}