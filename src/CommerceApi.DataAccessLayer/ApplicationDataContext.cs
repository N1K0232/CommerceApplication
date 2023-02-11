using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using CommerceApi.Authentication;
using CommerceApi.DataAccessLayer.Entities.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace CommerceApi.DataAccessLayer;

public class ApplicationDataContext : AuthenticationDataContext, IDataContext, IDapperContext
{
    private static readonly MethodInfo setQueryFilter = typeof(ApplicationDataContext)
        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilter));

    private CancellationTokenSource cancellationTokenSource = null;
    private SqlConnection connection = null;

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options, ILogger<ApplicationDataContext> logger) : base(options, logger)
    {
        connection = new SqlConnection(Database.GetConnectionString());
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

    public Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity
    {
        return ExistsAsyncInternal<T>(x => x.Id == id);
    }

    public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
    {
        return ExistsAsyncInternal(predicate);
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
        cancellationTokenSource ??= new CancellationTokenSource();

        return strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            await action.Invoke().ConfigureAwait(false);
            await transaction.CommitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
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

        cancellationTokenSource ??= new CancellationTokenSource();
        return SaveChangesAsync(cancellationTokenSource.Token);
    }
#pragma warning restore IDE0007

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
        cancellationTokenSource ??= new CancellationTokenSource();

        var set = GetDataInternal<TEntity>();
        return set.AnyAsync(predicate, cancellationTokenSource.Token);
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            if (connection != null)
            {
                if (connection.State is ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }

                await connection.DisposeAsync();
                connection = null;
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, object parameter = null, IDbTransaction transaction = null, CommandType? commandType = null) where T : BaseEntity
    {
        await connection.OpenAsync();

        var result = await connection.QueryAsync<T>(query, parameter, transaction, commandType: commandType);
        return result;
    }

    public async Task ExecuteAsync(string query, object parameter = null, CommandType? commandType = null)
    {
        await connection.OpenAsync();
        await connection.ExecuteAsync(query, parameter, commandType: commandType);
    }
}