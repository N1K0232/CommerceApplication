using System.Text.Json.Serialization;
using CommerceApi.ClientContext;
using CommerceApi.ClientContext.Converters;
using CommerceApi.Swagger.Documentation;
using CommerceApi.Swagger.DocumentFilters;
using CommerceApi.Swagger.OperationFilters;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TinyHelpers.Json.Serialization;

namespace CommerceApi.Swagger.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string xmlFile)
    {
        var supportedCultures = new[] { "en", "it", "de", "fr" };
        var localizationOptions = new RequestLocalizationOptions()
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures)
            .SetDefaultCulture(supportedCultures[0]);

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SupportedCultures = localizationOptions.SupportedCultures;
            options.SupportedUICultures = localizationOptions.SupportedUICultures;
            options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
        });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
            options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
        });

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSingleton<IActionResultExecutor<ObjectResult>, ProblemDetailsResultExecutor>();

        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<FormFileOperationFilter>();
            options.OperationFilter<AuthResponseOperationFilter>();
            options.OperationFilter<DefaultResponseOperationFilter>();
            options.OperationFilter<CultureAwareOperationFilter>();
            options.OperationFilter<SwaggerDefaultValues>();
            options.AddClientContextOperationFilter();

            options.DocumentFilter<ProblemDetailsDocumentFilter>();

            options.MapType<DateOnly>("string", "date");
            options.MapType<TimeOnly>("string", "date", TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm:ss"));

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert JWT token with the \"Bearer \" prefix",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{xmlFile}.xml");
            options.IncludeXmlComments(xmlPath);

            options.CustomOperationIds(api => $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}");
            options.UseAllOfToExtendReferenceSchemas();
        })
        .AddFluentValidationRulesToSwagger(options =>
        {
            options.SetNotNullableIfMinLengthGreaterThenZero = true;
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                options.RoutePrefix = string.Empty;
            }
        });

        return app;
    }
}