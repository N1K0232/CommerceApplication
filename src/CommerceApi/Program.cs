using System.ComponentModel;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using CommerceApi.Authentication;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Settings;
using CommerceApi.Authorization.Extensions;
using CommerceApi.Authorization.Handlers;
using CommerceApi.Authorization.Requirements;
using CommerceApi.BusinessLayer.BackgroundServices;
using CommerceApi.BusinessLayer.RemoteServices;
using CommerceApi.BusinessLayer.RemoteServices.Interfaces;
using CommerceApi.BusinessLayer.Services;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.BusinessLayer.StartupServices;
using CommerceApi.Client;
using CommerceApi.ClientContext;
using CommerceApi.ClientContext.TypeConverters;
using CommerceApi.DataAccessLayer;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Extensions;
using CommerceApi.DataAccessLayer.Handlers;
using CommerceApi.DataAccessLayer.Handlers.Common;
using CommerceApi.DataProtectionLayer;
using CommerceApi.DataProtectionLayer.Extensions;
using CommerceApi.Security.Extensions;
using CommerceApi.Swagger.Extensions;
using CommerceApi.TenantContext;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OperationResults.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

ConfigureServices(builder.Services, builder.Configuration, builder.Host);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
Configure(app, provider);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host)
{
    TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));
    TypeDescriptor.AddAttributes(typeof(TimeOnly), new TypeConverterAttribute(typeof(TimeOnlyTypeConverter)));

    host.UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddProblemDetails(options =>
    {
        options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError)
        {
            Detail = ex.Message,
            Instance = ex.GetType().ToString(),
            Type = ex.GetType().ToString(),
            Title = ex.Source
        });
    });

    services.AddHttpContextAccessor();
    services.AddClientContextAccessor();
    services.AddTenantContextAccessor(options =>
    {
        var tenants = Configure<IList<Tenant>>("Tenants");
        var availableTenants = tenants.Select(t => t.Name).ToList();

        options.AvailableTenants = availableTenants;
    });

    services.AddMemoryCache();
    services.AddOperationResult();
    services.AddPasswordHasher();
    services.AddPathGenerator();
    services.AddStringHasher();
    services.AddDataProtection().PersistKeysToDbContext<DataProtectionDbContext>();
    services.AddDataProtector();
    services.AddTimeLimitedDataProtector();

    services.AddMapperProfiles();
    services.AddValidators();

    services.AddSwaggerDocumentation(Assembly.GetExecutingAssembly().GetName().Name);
    services.AddScoped<IEmailClient, EmailClient>();

    var connectionString = configuration.GetConnectionString("SqlConnection");
    services.AddSqlServer<ApplicationDbContext>(connectionString, options =>
    {
        options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    services.AddSqlServer<AuthenticationDbContext>(connectionString, options =>
    {
        options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    services.AddSqlServer<DataProtectionDbContext>(connectionString, options =>
    {
        options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
    });

    services.AddScoped<IDataProtectionKeyContext>(provider => provider.GetRequiredService<DataProtectionDbContext>());
    services.AddScoped<IReadOnlyDataContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
    services.AddScoped<IDataContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

    services.AddSqlContext(options =>
    {
        options.ConnectionString = connectionString;
        options.CommandTimeout = configuration.GetValue<int>("AppSettings:CommandTimeout");
    });

    services.AddScoped<IDbConnectionHandler, SqlConnectionHandler>();

    var jwtSettings = Configure<JwtSettings>(nameof(JwtSettings));
    services.AddAuthentication(jwtSettings);

    services.AddAuthorization(options =>
    {
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new UserActiveRequirement());

        var policy = policyBuilder.Build();
        options.FallbackPolicy = policy;

        options.AddPolicy("UserActive", policy =>
        {
            policy.Requirements.Add(new UserActiveRequirement());
        });

        options.AddPolicy("Administrator", policy =>
        {
            policy.RequireRole(RoleNames.Administrator);
            policy.Requirements.Add(new UserActiveRequirement());
        });

        options.AddPolicy("PowerUser", policy =>
        {
            policy.RequireRole(RoleNames.PowerUser);
            policy.Requirements.Add(new UserActiveRequirement());
        });

        options.AddPolicy("Tenant", policy =>
        {
            policy.Requirements.Add(new TenantRequirement());
        });
    });

    services.AddAuthorizationHandler<UserActiveHandler>();
    services.AddAuthorizationHandler<TenantHandler>();

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

    //add services
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<ICartService, CartService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IConstructorService, ConstructorService>();
    services.AddScoped<ICouponService, CouponService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IIdentityService, IdentityService>();
    services.AddScoped<IImageService, ImageService>();
    services.AddScoped<IInvoiceService, InvoiceService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<ISupplierService, SupplierService>();
    services.AddScoped<IUserService, UserService>();

    //add remote services
    services.AddScoped<IPdfService, PdfService>();
    services.AddScoped<IKeyService, KeyService>();

    //add startup services
    services.AddHostedService<ApplicationStartupService>();
    services.AddHostedService<AuthenticationStartupService>();

    //add background services
    services.AddHostedService<SqlConnectionControlService>();

    //add storage providers
    var azureStorageConnectionString = configuration.GetConnectionString("StorageConnection");
    var storageFolder = configuration.GetValue<string>("AppSettings:StorageFolder");
    if (!string.IsNullOrWhiteSpace(azureStorageConnectionString))
    {
        services.AddAzureStorageProvider(options =>
        {
            options.ConnectionString = azureStorageConnectionString;
            options.ContainerName = storageFolder;
        });
    }
    else
    {
        services.AddFileSystemStorageProvider(options =>
        {
            options.StorageFolder = storageFolder;
        });
    }

    T Configure<T>(string sectionName) where T : class
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<T>();

        services.Configure<T>(section);
        return settings;
    }
}

void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
{
    app.UseProblemDetails();
    app.UseSwaggerDocumentation(provider);

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();

    app.UseClientContext();
    app.UseTenantContext();

    app.UseRequestLocalization();

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