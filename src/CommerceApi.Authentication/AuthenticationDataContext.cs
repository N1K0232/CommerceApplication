using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace CommerceApi.Authentication;

public partial class AuthenticationDataContext
    : IdentityDbContext<ApplicationUser,
      ApplicationRole,
      Guid,
      IdentityUserClaim<Guid>,
      ApplicationUserRole,
      IdentityUserLogin<Guid>,
      IdentityRoleClaim<Guid>,
      IdentityUserToken<Guid>>
{
    private readonly ValueConverter<string, string> trimStringConverter = new(v => v.Trim(), v => v.Trim());

    public AuthenticationDataContext(DbContextOptions options, ILogger logger) : base(options)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var savedEntries = await base.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("saved {savedEntries} rows in the database", savedEntries);

            return savedEntries;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogError(ex, "error while saving");
            throw ex;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "error while saving");
            throw ex;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        OnConfiguringCore(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }

    partial void OnConfiguringCore(DbContextOptionsBuilder optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnModelCreatingCore(modelBuilder);
    }

    partial void OnModelCreatingCore(ModelBuilder modelBuilder);
}