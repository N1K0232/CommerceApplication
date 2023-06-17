using System.ComponentModel;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using CommerceApi.Authentication;
using CommerceApi.Authentication.Common;
using CommerceApi.Authentication.Entities;
using CommerceApi.Authentication.Managers;
using CommerceApi.Authentication.Settings;
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
using CommerceApi.DataProtectionLayer.Extensions;
using CommerceApi.DataProtectionLayer.Services;
using CommerceApi.DataProtectionLayer.Services.Interfaces;
using CommerceApi.Extensions;
using CommerceApi.Security.Extensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OperationResults.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
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
        options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
    });

    services.AddHttpContextAccessor();
    services.AddClientContextAccessor();
    services.AddMemoryCache();
    services.AddOperationResult();
    services.AddUserClaimService();
    services.AddPasswordHasher();
    services.AddPathGenerator();
    services.AddStringHasher();
    services.AddDataProtection().PersistKeysToDbContext<CommerceApplicationDbContext>();
    services.AddDataProtector();
    services.AddTimeLimitedDataProtector();

    services.AddMapperProfiles();
    services.AddValidators();

    services.AddSwaggerDocumentation();
    services.AddScoped<IEmailClient, EmailClient>();

    var sqlConnectionString = configuration.GetConnectionString("SqlConnection");
    services.AddSqlServer<CommerceApplicationDbContext>(sqlConnectionString);
    services.AddSqlServer<AuthenticationDbContext>(sqlConnectionString);

    services.AddScoped<IReadOnlyDataContext>(provider => provider.GetRequiredService<CommerceApplicationDbContext>());
    services.AddScoped<ICommerceApplicationDbContext>(provider => provider.GetRequiredService<CommerceApplicationDbContext>());
    services.AddScoped<IDataProtectionKeyContext>(provider => provider.GetRequiredService<CommerceApplicationDbContext>());

    services.AddSqlContext(options =>
    {
        options.ConnectionString = sqlConnectionString;
        options.CommandTimeout = configuration.GetValue<int>("AppSettings:CommandTimeout");
    });

    services.AddScoped<ApplicationRoleManager>();
    services.AddScoped<ApplicationUserManager>();
    services.AddScoped<ApplicationSignInManager>();

    services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(3);
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddDefaultTokenProviders()
    .AddRoleManager<ApplicationRoleManager>()
    .AddUserManager<ApplicationUserManager>()
    .AddSignInManager<ApplicationSignInManager>();

    var jwtSettingsSection = configuration.GetSection(nameof(JwtSettings));
    var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
    services.Configure<JwtSettings>(jwtSettingsSection);

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
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
    services.AddHealthChecks().AddAsyncCheck("sql", async () =>
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
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
    services.AddScoped<IIdentityService, IdentityService>();
    services.AddScoped<IImageService, ImageService>();
    services.AddScoped<IInvoiceService, InvoiceService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<ISupplierService, SupplierService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICouponService, CouponService>();
    services.AddScoped<IKeyService, KeyService>();

    //add remote services
    services.AddScoped<IPdfService, PdfService>();

    //add startup services
    services.AddHostedService<ApplicationStartupService>();
    services.AddHostedService<AuthenticationStartupService>();

    //add background services
    services.AddHostedService<SqlConnectionControlService>();

    services.AddScoped<IDataProtectionService, DataProtectionService>();
    services.AddScoped<ITimeLimitedDataProtectionService, TimeLimitedDataProtectionService>();

    var storageConnectionString = configuration.GetConnectionString("StorageConnection");
    if (!string.IsNullOrWhiteSpace(storageConnectionString))
    {
        var containerName = configuration.GetValue<string>("AppSettings:ContainerName");
        services.AddAzureStorageProvider(options =>
        {
            options.ConnectionString = storageConnectionString;
            options.ContainerName = containerName;
        });
    }
    else
    {
        var storageFolder = configuration.GetValue<string>("AppSettings:StorageFolder");
        services.AddFileSystemStorageProvider(options =>
        {
            options.StorageFolder = storageFolder;
        });
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