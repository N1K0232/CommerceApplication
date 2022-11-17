using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommerceApi.Authentication;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Models;
using CommerceApi.Authentication.Settings;
using CommerceApi.Authentication.StartupTasks;
using CommerceApi.Authorization.Handlers;
using CommerceApi.Authorization.Requirements;
using CommerceApi.BusinessLayer.Services;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
using CommerceApi.Documentation;
using CommerceApi.Security;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Host);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
Configure(app, provider);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host)
{
    host.UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddProblemDetails(options =>
    {
        options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
    });

    services.AddHttpContextAccessor();
    services.AddMemoryCache();
    services.AddOperationResult();
    services.AddUserClaimService();

    services.AddMapperProfiles();
    services.AddValidators();

    services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeSpanTicksConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
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
    services.AddSwaggerGen(options =>
    {
        options.OperationFilter<SwaggerDefaultValues>();

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

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.CustomOperationIds(api => $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}");
        options.UseAllOfToExtendReferenceSchemas();
    })
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });

    services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
    });

    var hashedConnectionString = configuration.GetConnectionString("SqlConnection");
    var connectionString = StringHasher.GetString(hashedConnectionString);

    services.AddSqlServer<ApplicationDataContext>(connectionString);
    services.AddScoped<IDataContext>(provider => provider.GetRequiredService<ApplicationDataContext>());

    services.AddSqlServer<AuthenticationDataContext>(connectionString);
    services.AddIdentity<AuthenticationUser, AuthenticationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<AuthenticationDataContext>()
    .AddDefaultTokenProviders();

    var jwtSection = configuration.GetSection(nameof(JwtSettings));
    var jwtSettings = jwtSection.Get<JwtSettings>();
    services.Configure<JwtSettings>(jwtSection);

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = jwtSettings.Audience,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddAuthorization(options =>
    {
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new UserActiveRequirement());

        var policy = policyBuilder.Build();
        options.FallbackPolicy = options.DefaultPolicy = policy;

        options.AddPolicy("UserActive", policy =>
        {
            policy.Requirements.Add(new UserActiveRequirement());
        });
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

    services.AddHealthChecks().AddAsyncCheck("sql", async () =>
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }

        return HealthCheckResult.Healthy();
    });

    services.TryAddScoped<IUserService, UserService>();
    services.TryAddScoped<ICategoryService, CategoryService>();
    services.TryAddScoped<IProductService, ProductService>();
    services.TryAddScoped<IOrderService, OrderService>();

    services.TryAddScoped<IAuthenticationService, AuthenticationService>();

    var adminUserSection = configuration.GetSection(nameof(AdministratorUser));
    services.Configure<AdministratorUser>(adminUserSection);

    services.AddHostedService<AuthenticationStartupTask>();
}

void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
{
    app.UseProblemDetails();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
            options.RoutePrefix = string.Empty;
        }
    });

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();

        endpoints.MapHealthChecks("/status",
                new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonSerializer.Serialize(
                            new
                            {
                                Status = report.Status.ToString(),
                                Details = report.Entries.Select(e => new
                                {
                                    Service = e.Key,
                                    Status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                                    Description = e.Value.Description
                                })
                            });

                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });
    });
}