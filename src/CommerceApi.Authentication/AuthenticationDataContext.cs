using CommerceApi.Authentication.Configurations;
using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace CommerceApi.Authentication;

public class AuthenticationDataContext
    : IdentityDbContext<AuthenticationUser,
      AuthenticationRole,
      Guid,
      IdentityUserClaim<Guid>,
      AuthenticationUserRole,
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var savedEntries = await base.SaveChangesAsync(cancellationToken);
            logger.LogInformation("saved {0} rows in the database", savedEntries);

            return savedEntries;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "error while saving");
            throw ex;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "error while saving");
            throw ex;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AuthenticationUserConfiguration());
        modelBuilder.ApplyConfiguration(new AuthenticationUserRoleConfiguration());

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    modelBuilder.Entity(entity.Name)
                    .Property(property.Name)
                    .HasConversion(trimStringConverter);
                }
            }
        }
    }
}