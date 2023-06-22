using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Security.Cryptography;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace CommerceApi.DataAccessLayer;

public partial class ApplicationDbContext
{
    private static readonly MethodInfo _setQueryFilterOnDeletableEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private static readonly MethodInfo _setQueryFilterOnTenantEntity = typeof(ApplicationDbContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private readonly IUserClaimService _claimService;
    private readonly ValueConverter<string, string> _trimStringConverter = new ValueConverter<string, string>(v => v.Trim(), v => v.Trim());

    private ValidationContext _validationContext = null;
    private IDbContextTransaction _transaction = null;

    private RandomNumberGenerator _generator = null;

    private void DeleteInternal<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        var set = Set<TEntity>();
        if (entity is DeletableEntity)
        {
            set.Attach(entity);
        }

        set.Remove(entity);
    }

    private async Task<DbCommand> LoadStoredProcedureInternalAsync(string procedureName)
    {
        _tokenSource ??= new CancellationTokenSource();

        var connection = Database.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            await connection.OpenAsync(_tokenSource.Token).ConfigureAwait(false);
        }

        var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;

        return command;
    }

    private async Task ExecuteTransactionCoreAsync(Func<Task> action)
    {
        _tokenSource ??= new CancellationTokenSource();
        var token = _tokenSource.Token;

        _transaction = await Database.BeginTransactionAsync(token).ConfigureAwait(false);
        await action.Invoke().ConfigureAwait(false);
        await _transaction.CommitAsync(token).ConfigureAwait(false);
    }

    private Task ValidateAsync(BaseEntity entity)
    {
        try
        {
            _validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, _validationContext, true);

            return Task.CompletedTask;
        }
        catch (ValidationException ex)
        {
            return Task.FromException(ex);
        }
    }

    private string GenerateSecurityStamp()
    {
        var bytes = new byte[256];
        _generator = RandomNumberGenerator.Create();
        _generator.GetBytes(bytes);

        var securityStamp = Convert.ToBase64String(bytes).ToUpperInvariant();
        return securityStamp;
    }

    private IEnumerable<EntityEntry> GetEntries()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        return entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }

    partial void OnConfiguringCore(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var sqlConnectionString = _configuration.GetConnectionString("SqlConnection");
            optionsBuilder.UseSqlServer(sqlConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
            });
        }

        var interceptors = GetInterceptorsFromAssembly(Assembly.GetExecutingAssembly());
        optionsBuilder.AddInterceptors(interceptors);
        optionsBuilder.UseMemoryCache(_memoryCache);
    }

    partial void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SetQueryFilter(modelBuilder);
        SetStringConverter(modelBuilder);
    }

    private void SetQueryFilter(ModelBuilder modelBuilder)
    {
        var entries = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(DeletableEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        var types = entries.Select(t => t.ClrType);
        foreach (var type in types)
        {
            var methods = SetGlobalQueryFilter(type);
            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }
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

    private static IEnumerable<MethodInfo> SetGlobalQueryFilter(Type entityType)
    {
        var result = new List<MethodInfo>();

        if (typeof(DeletableEntity).IsAssignableFrom(entityType))
        {
            result.Add(_setQueryFilterOnDeletableEntity);
        }

        if (typeof(TenantEntity).IsAssignableFrom(entityType))
        {
            result.Add(_setQueryFilterOnTenantEntity);
        }

        return result;
    }

    private static IEnumerable<IInterceptor> GetInterceptorsFromAssembly(Assembly assembly)
    {
        var interceptors = new List<IInterceptor>();
        var interceptorTypes = assembly.GetTypes().Where(t => t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IInterceptor)));
        foreach (var interceptorType in interceptorTypes)
        {
            var interceptor = (IInterceptor)Activator.CreateInstance(interceptorType);
            interceptors.Add(interceptor);
        }

        return interceptors;
    }

    private void SetQueryFilterOnDeletableEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : DeletableEntity
    {
        var builder = modelBuilder.Entity<TEntity>();
        builder.HasQueryFilter(e => !e.IsDeleted && e.DeletedDate == null && e.DeletedTime == null);
    }

    private void SetQueryFilterOnTenantEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        var tenantId = _claimService.GetTenantId();
        var builder = modelBuilder.Entity<TEntity>();
        builder.HasQueryFilter(e => e.TenantId == tenantId);
    }
}