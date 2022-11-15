using System.Linq.Expressions;
using System.Reflection;
using CommerceApi.Authentication;
using CommerceApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace CommerceApi.DataAccessLayer;

public class ApplicationDataContext : AuthenticationDataContext, IDataContext
{
    private static readonly MethodInfo setQueryFilter = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilter));

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options, ILogger<ApplicationDataContext> logger) : base(options, logger)
    {
    }

    public void Delete<T>(T entity) where T : BaseEntity
    {
        var set = Set<T>();
        set.Remove(entity);
    }

    public void Delete<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var set = Set<T>();
        set.RemoveRange(entities);
    }

    public void Edit<T>(T entity) where T : BaseEntity
    {
        var set = Set<T>();
        set.Update(entity);
    }

    public void Edit<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var set = Set<T>();
        set.UpdateRange(entities);
    }

    public Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity
    {
        return ExistsAsyncInternal<T>(x => x.Id == id);
    }

    public Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
    {
        return ExistsAsyncInternal<T>(predicate);
    }

    public Task<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var set = Set<T>();
        return set.FindAsync(id).AsTask();
    }

    public IQueryable<T> GetData<T>(bool ignoreAutoIncludes = true, bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity
    {
        return GetDataInternal<T>(ignoreAutoIncludes, ignoreQueryFilters, trackingChanges);
    }

    public void Create<T>(T entity) where T : BaseEntity
    {
        var set = Set<T>();
        set.Add(entity);
    }

    public void Create<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var set = Set<T>();
        set.AddRange(entities);
    }

    public Task<int> SaveAsync()
    {
        using var tokenSource = new CancellationTokenSource();
        var cancellationToken = tokenSource.Token;
        return SaveChangesAsync(cancellationToken);
    }

    public Task ExecuteTransactionAsync() => throw new NotImplementedException();

#pragma warning disable IDE0007 // Use implicit type
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = GetEntries();

        foreach (var entry in entries)
        {
            BaseEntity baseEntity = entry.Entity as BaseEntity;

            if (entry.State is EntityState.Added)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                }

                baseEntity.CreationDate = DateTime.UtcNow;
                baseEntity.UpdatedDate = null;
            }

            if (entry.State is EntityState.Modified)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                }

                baseEntity.UpdatedDate = DateTime.UtcNow;
            }

            if (entry.State is EntityState.Deleted)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    entry.State = EntityState.Modified;

                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedDate = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
#pragma warning restore IDE0007 // Use implicit type

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var entries = builder.Model.GetEntityTypes()
            .Where(t => typeof(DeletableEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var type in entries.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFilter(type);

            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { builder });
            }
        }
    }

    private IEnumerable<EntityEntry> GetEntries()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().IsSubclassOf(typeof(BaseEntity))).ToList();

        return entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }

#pragma warning disable IDE0007 // Use implicit type
    private static IEnumerable<MethodInfo> SetGlobalQueryFilter(Type entityType)
    {
        Type deletableEntityType = typeof(DeletableEntity);
        var result = new List<MethodInfo>();

        if (deletableEntityType.IsAssignableFrom(entityType))
        {
            result.Add(setQueryFilter);
        }

        return result;
    }
#pragma warning restore IDE0007 // Use implicit type

    private void SetQueryFilter<T>(ModelBuilder builder) where T : DeletableEntity
    {
        builder.Entity<T>().HasQueryFilter(x => !x.IsDeleted && x.DeletedDate == null);
    }

    private IQueryable<T> GetDataInternal<T>(bool ignoreAutoIncludes = true, bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>().AsQueryable();

        if (ignoreAutoIncludes)
        {
            set = set.IgnoreAutoIncludes();
        }

        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ?
            set.AsTracking() :
            set.AsNoTrackingWithIdentityResolution();
    }

    private Task<bool> ExistsAsyncInternal<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
    {
        var set = GetDataInternal<T>(ignoreAutoIncludes: false);
        return set.AnyAsync(predicate);
    }
}