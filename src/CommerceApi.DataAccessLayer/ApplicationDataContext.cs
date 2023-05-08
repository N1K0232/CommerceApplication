﻿using System.Reflection;
using CommerceApi.Authentication;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CommerceApi.DataAccessLayer;

public partial class ApplicationDataContext : AuthenticationDataContext, IDataContext
{
    private readonly IMemoryCache memoryCache;
    private CancellationTokenSource tokenSource = null;

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options,
        ILogger<ApplicationDataContext> logger,
        IUserClaimService claimService,
        IMemoryCache memoryCache) : base(options, logger)
    {
        this.claimService = claimService;
        this.memoryCache = memoryCache;
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
    public async Task SaveAsync()
    {
        var entries = GetEntries(EntityState.Added | EntityState.Modified | EntityState.Modified);
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

            await ValidateAsync(baseEntity);
        }

        tokenSource ??= new CancellationTokenSource();
        await SaveChangesAsync(tokenSource.Token);
    }
#pragma warning restore IDE0007

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var interceptors = GetInterceptorsFromAssembly(Assembly.GetExecutingAssembly());
        optionsBuilder.AddInterceptors(interceptors);

        optionsBuilder.UseMemoryCache(memoryCache);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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