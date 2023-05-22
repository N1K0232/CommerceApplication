using CommerceApi.Authentication;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TinyHelpers.Extensions;

namespace CommerceApi.DataAccessLayer;

public partial class ApplicationDataContext : AuthenticationDataContext, IDataContext
{
    private readonly IConfiguration configuration;
    private readonly IMemoryCache memoryCache;

    private CancellationTokenSource tokenSource = null;

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options,
        ILogger<ApplicationDataContext> logger,
        IUserClaimService claimService,
        IConfiguration configuration,
        IMemoryCache memoryCache) : base(options, logger)
    {
        this.claimService = claimService;
        this.configuration = configuration;
        this.memoryCache = memoryCache;
    }


    public void Create<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        Set<TEntity>().Add(entity);
    }

    void IDataContext.Update<TEntity>(TEntity entity)
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

    public async ValueTask<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity
    {
        tokenSource ??= new CancellationTokenSource();

        var set = Set<TEntity>();

        var entity = await set.FindAsync(id, tokenSource.Token).ConfigureAwait(false);
        return entity;
    }

    public IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
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

#pragma warning disable IDE0007 //Use implicit type
    public async Task SaveAsync()
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

                if (baseEntity is FileEntity fileEntity)
                {
                    fileEntity.DownloadFileName = $"{Guid.NewGuid()}_{fileEntity.FileName}";
                }

                baseEntity.CreationDate = DateTime.UtcNow.ToDateOnly();
                baseEntity.CreationTime = DateTime.UtcNow.ToTimeOnly();

                baseEntity.LastModificationDate = null;
                baseEntity.LastModificationTime = null;
            }

            if (entry.State is EntityState.Modified)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                    deletableEntity.DeletedTime = null;
                }

                baseEntity.LastModificationDate = DateTime.UtcNow.ToDateOnly();
                baseEntity.LastModificationTime = DateTime.UtcNow.ToTimeOnly();
            }

            if (entry.State is EntityState.Deleted)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    entry.State = EntityState.Modified;

                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedDate = DateTime.UtcNow.ToDateOnly();
                    deletableEntity.DeletedTime = DateTime.UtcNow.ToTimeOnly();
                }
            }

            await ValidateAsync(baseEntity);
        }

        tokenSource ??= new CancellationTokenSource();
        await SaveChangesAsync(tokenSource.Token);
    }
#pragma warning restore IDE0007

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(() => ExecuteTransactionCoreAsync(action));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ConfigureConventionsCore(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    partial void ConfigureConventionsCore(ModelConfigurationBuilder configurationBuilder);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        OnConfiguringCore(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }

    partial void OnConfiguringCore(DbContextOptionsBuilder optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingCore(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    partial void OnModelCreatingCore(ModelBuilder modelBuilder);

    public override void Dispose()
    {
        tokenSource.Dispose();

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}