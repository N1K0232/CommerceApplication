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


    public Task CreateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        if (entity is DeletableEntity deletableEntity)
        {
            deletableEntity.IsDeleted = false;
            deletableEntity.DeletedDate = null;
        }

        entity.CreationDate = DateTime.UtcNow;
        entity.UpdatedDate = null;

        Set<TEntity>().Add(entity);
        return SaveChangesAsync();
    }

    public Task UpdateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        if (entity is DeletableEntity deletableEntity)
        {
            deletableEntity.IsDeleted = false;
            deletableEntity.DeletedDate = null;
        }

        entity.UpdatedDate = DateTime.UtcNow;

        Set<TEntity>().Update(entity);
        return SaveChangesAsync();
    }

    public Task DeleteAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        Set<TEntity>().Remove(entity);
        return SaveChangesAsync();
    }

    public Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        Set<TEntity>().RemoveRange(entities);
        return SaveChangesAsync();
    }

    public Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity
    {
        return ExistsAsyncInternal<T>(x => x.Id == id);
    }

    public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
    {
        return ExistsAsyncInternal(predicate);
    }

    public Task<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity
    {
        var set = Set<TEntity>();
        return set.FindAsync(id).AsTask();
    }

    public IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        return GetDataInternal<TEntity>(ignoreQueryFilters, trackingChanges);
    }

    public Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync().ConfigureAwait(false);
            await action.Invoke().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = GetEntries();

        foreach (var entry in entries)
        {
            if (entry.State is EntityState.Deleted && entry.Entity is DeletableEntity deletableEntity)
            {
                entry.State = EntityState.Modified;
                deletableEntity.IsDeleted = true;
                deletableEntity.DeletedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

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
            .Where(e => e.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        return entries.Where(e => e.State is EntityState.Deleted);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFilter(Type entityType)
    {
        var result = new List<MethodInfo>();

        if (typeof(DeletableEntity).IsAssignableFrom(entityType))
        {
            result.Add(setQueryFilter);
        }

        return result;
    }

    private void SetQueryFilter<TEntity>(ModelBuilder builder) where TEntity : DeletableEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(x => !x.IsDeleted && x.DeletedDate == null);
    }

    private IQueryable<TEntity> GetDataInternal<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        var set = Set<TEntity>().AsQueryable();

        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ?
            set.AsTracking() :
            set.AsNoTrackingWithIdentityResolution();
    }

    private Task<bool> ExistsAsyncInternal<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
    {
        var set = GetDataInternal<TEntity>();
        return set.AnyAsync(predicate);
    }
}