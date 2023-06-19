using System.Reflection;
using CommerceApi.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace CommerceApi.Authentication;

public partial class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser,
      ApplicationRole,
      Guid,
      IdentityUserClaim<Guid>,
      ApplicationUserRole,
      IdentityUserLogin<Guid>,
      IdentityRoleClaim<Guid>,
      IdentityUserToken<Guid>>
{
    private DbSet<ApplicationUser> _users;
    private DbSet<ApplicationRole> _roles;

    private DbSet<ApplicationUserRole> _userRoles;
    private DbSet<IdentityUserClaim<Guid>> _userClaims;
    private DbSet<IdentityRoleClaim<Guid>> _roleClaims;

    private DbSet<IdentityUserLogin<Guid>> _userLogins;
    private DbSet<IdentityUserToken<Guid>> _userTokens;

    private DbSet<Address> _addresses;

    private readonly ValueConverter<string, string> _trimStringConverter = new(v => v.Trim(), v => v.Trim());
    private readonly ILogger<AuthenticationDbContext> _logger;

    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options, ILogger<AuthenticationDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    public override DbSet<ApplicationUser> Users
    {
        get
        {
            return _users ?? base.Users;
        }
        set
        {
            var users = value ?? Set<ApplicationUser>();
            if (_users != value)
            {
                _users = users;
                base.Users = users;
            }
        }
    }

    public override DbSet<ApplicationRole> Roles
    {
        get
        {
            return _roles ?? base.Roles;
        }
        set
        {
            var roles = value ?? Set<ApplicationRole>();
            if (_roles != roles)
            {
                _roles = roles;
                base.Roles = roles;
            }
        }
    }

    public override DbSet<ApplicationUserRole> UserRoles
    {
        get
        {
            return _userRoles ?? base.UserRoles;
        }
        set
        {
            var userRoles = value ?? Set<ApplicationUserRole>();
            if (_userRoles != userRoles)
            {
                _userRoles = userRoles;
                base.UserRoles = userRoles;
            }
        }
    }

    public override DbSet<IdentityUserClaim<Guid>> UserClaims
    {
        get
        {
            return _userClaims ?? base.UserClaims;
        }
        set
        {
            var userClaims = value ?? Set<IdentityUserClaim<Guid>>();
            if (_userClaims != userClaims)
            {
                _userClaims = userClaims;
                base.UserClaims = userClaims;
            }
        }
    }

    public override DbSet<IdentityRoleClaim<Guid>> RoleClaims
    {
        get
        {
            return _roleClaims ?? base.RoleClaims;
        }
        set
        {
            var roleClaims = value ?? Set<IdentityRoleClaim<Guid>>();
            if (_roleClaims != roleClaims)
            {
                _roleClaims = roleClaims;
                base.RoleClaims = roleClaims;
            }
        }
    }

    public override DbSet<IdentityUserLogin<Guid>> UserLogins
    {
        get
        {
            return _userLogins ?? base.UserLogins;
        }
        set
        {
            var userLogins = value ?? Set<IdentityUserLogin<Guid>>();
            if (_userLogins != userLogins)
            {
                _userLogins = userLogins;
                base.UserLogins = userLogins;
            }
        }
    }

    public override DbSet<IdentityUserToken<Guid>> UserTokens
    {
        get
        {
            return _userTokens ?? base.UserTokens;
        }
        set
        {
            var userTokens = value ?? Set<IdentityUserToken<Guid>>();
            if (_userTokens != userTokens)
            {
                _userTokens = userTokens;
                base.UserTokens = userTokens;
            }
        }
    }

    public virtual DbSet<Address> Addresses
    {
        get
        {
            return _addresses;
        }
        set
        {
            var addresses = value ?? Set<Address>();
            if (_addresses != addresses)
            {
                _addresses = addresses;
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var savedEntries = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("saved {savedEntries} rows in the database", savedEntries);

            return savedEntries;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "error while updating database");
            throw ex;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "error while updating database");
            throw ex;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            throw ex;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            throw ex;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SetStringConverter(modelBuilder);
    }

    private void SetStringConverter(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    modelBuilder.Entity(entity.Name)
                    .Property(property.Name)
                    .HasConversion(_trimStringConverter);
                }
            }
        }
    }
}