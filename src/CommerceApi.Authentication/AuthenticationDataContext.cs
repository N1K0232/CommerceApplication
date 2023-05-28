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
    private readonly ILogger<AuthenticationDataContext> logger;

    public AuthenticationDataContext(DbContextOptions options, ILogger<AuthenticationDataContext> logger) : base(options)
    {
        this.logger = logger;
    }

    public ILogger Logger => logger;

    public virtual DbSet<Address> Addresses { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var savedEntries = await base.SaveChangesAsync(cancellationToken);
            logger.LogInformation("saved {savedEntries} rows in the database", savedEntries);

            return savedEntries;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "error while updating database");
            throw ex;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "error while updating");
            throw ex;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Request timeout");
            throw ex;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request timeout");
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