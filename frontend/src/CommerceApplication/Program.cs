using CommerceApplication.BusinessLayer.Settings;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Configure(app, app.Environment);

app.MapRazorPages();
await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var appSettings = Configure<AppSettings>(nameof(AppSettings));
    services.AddRazorPages();

    T Configure<T>(string sectionName) where T : class
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<T>();

        services.Configure<T>(section);
        return settings;
    }
}

void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
{
    if (!environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseAuthorization();
}