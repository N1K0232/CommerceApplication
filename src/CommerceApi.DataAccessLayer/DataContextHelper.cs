using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace CommerceApi.DataAccessLayer;

public partial class ApplicationDataContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private static readonly MethodInfo setQueryFilterOnTenantEntity = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private readonly IUserClaimService claimService;
    private ValidationContext validationContext = null;

    private Task ValidateAsync(object entity)
    {
        try
        {
            validationContext ??= new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext, true);

            return Task.CompletedTask;
        }
        catch (ValidationException ex)
        {
            return Task.FromException(ex);
        }
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
            var sqlConnectionString = configuration.GetConnectionString("SqlConnection");
            optionsBuilder.UseSqlServer(sqlConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
            });
        }

        var interceptors = GetInterceptorsFromAssembly(Assembly.GetExecutingAssembly());
        optionsBuilder.AddInterceptors(interceptors);
        optionsBuilder.UseMemoryCache(memoryCache);
    }

    partial void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var entries = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(DeletableEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var type in entries.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFilter(type);

            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFilter(Type entityType)
    {
        var result = new List<MethodInfo>();

        if (typeof(DeletableEntity).IsAssignableFrom(entityType))
        {
            result.Add(setQueryFilterOnDeletableEntity);
        }

        if (typeof(TenantEntity).IsAssignableFrom(entityType))
        {
            result.Add(setQueryFilterOnTenantEntity);
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
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted && e.DeletedDate == null);
    }

    private void SetQueryFilterOnTenantEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        var tenantId = claimService.GetTenantId();
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenantId);
    }
}