using System.Data;
using System.Reflection;
using CommerceApi.Authentication;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace CommerceApi.DataAccessLayer;

public class ApplicationDataContext : AuthenticationDataContext, IDataContext
{
    private static readonly MethodInfo setQueryFilterOnDeletableEntity = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnDeletableEntity));

    private static readonly MethodInfo setQueryFilterOnTenantEntity = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private readonly IUserClaimService claimService;
    private CancellationTokenSource tokenSource = null;

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options,
        ILogger<ApplicationDataContext> logger,
        IUserClaimService claimService) : base(options, logger)
    {
        this.claimService = claimService;
    }


    public void Create<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<TEntity>().Add(entity);
    }

    public void Edit<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<TEntity>().Update(entity);
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<TEntity>().Remove(entity);
    }

    public void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        Set<TEntity>().RemoveRange(entities);
    }

    public ValueTask<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity
    {
        var set = Set<TEntity>();
        return set.FindAsync(id);
    }

    public IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        return GetDataInternal<TEntity>(ignoreQueryFilters, trackingChanges);
    }

    public Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        tokenSource ??= new CancellationTokenSource();

        return strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync(tokenSource.Token).ConfigureAwait(false);
            await action.Invoke().ConfigureAwait(false);
            await transaction.CommitAsync(tokenSource.Token).ConfigureAwait(false);
        });
    }

#pragma warning disable IDE0007 //Use implicit type
    public Task SaveAsync()
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

        tokenSource ??= new CancellationTokenSource();
        return SaveChangesAsync(tokenSource.Token);
    }
#pragma warning restore IDE0007

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            result.Add(setQueryFilterOnDeletableEntity);
        }

        if (typeof(TenantEntity).IsAssignableFrom(entityType))
        {
            result.Add(setQueryFilterOnTenantEntity);
        }

        return result;
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

    public override void Dispose()
    {
        tokenSource.Dispose();

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}