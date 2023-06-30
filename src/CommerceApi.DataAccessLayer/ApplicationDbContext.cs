using System.Data.Common;
using System.Reflection;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Comparers;
using CommerceApi.DataAccessLayer.Converters;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TinyHelpers.Extensions;

namespace CommerceApi.DataAccessLayer;

public partial class ApplicationDbContext : DbContext, IDataContext
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ApplicationDbContext> _logger;

    private CancellationTokenSource _tokenSource = null;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger,
        IUserClaimService claimService,
        IConfiguration configuration,
        IMemoryCache memoryCache)
        : base(options)
    {
        _logger = logger;
        _claimService = claimService;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    public void Create<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var set = Set<TEntity>();
        set.Add(entity);
    }

    public void Create<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        var hasItems = entities?.Any() ?? false;
        if (!hasItems)
        {
            throw new ArgumentNullException(nameof(entities), "you must provide at least one entity");
        }

        var set = Set<TEntity>();
        set.AddRange(entities);
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        DeleteInternal(entity);
    }

    public void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        var hasItems = entities?.Any() ?? false;
        if (!hasItems)
        {
            throw new ArgumentNullException(nameof(entities), "you must provide at least one entity");
        }

        foreach (var entity in entities)
        {
            DeleteInternal(entity);
        }
    }

    void IDataContext.Update<TEntity>(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var set = Set<TEntity>();
        set.Attach(entity);
        set.Update(entity);
    }

    void IDataContext.Update<TEntity>(IEnumerable<TEntity> entities)
    {
        var hasItems = entities?.Any() ?? false;
        if (!hasItems)
        {
            throw new ArgumentNullException(nameof(entities), "you must provide at least one entity");
        }

        var set = Set<TEntity>();
        set.AttachRange(entities);
        set.UpdateRange(entities);
    }

    public async ValueTask<TEntity> GetAsync<TEntity>(params object[] keyValues) where TEntity : BaseEntity
    {
        _tokenSource ??= new CancellationTokenSource();

        var token = _tokenSource.Token;
        var set = Set<TEntity>();
        return await set.FindAsync(keyValues, token).ConfigureAwait(false);
    }

    public IQueryable<TEntity> Get<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        var set = Set<TEntity>().AsQueryable();
        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

#pragma warning disable IDE0007 //Use implicit type
    public async Task SaveAsync()
    {
        _tokenSource ??= new CancellationTokenSource();

        try
        {
            var token = _tokenSource.Token;
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
                        deletableEntity.DeletedTime = null;
                    }

                    baseEntity.SecurityStamp = GenerateSecurityStamp();
                    baseEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

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

                    baseEntity.SecurityStamp = GenerateSecurityStamp();
                    baseEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

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

                await ValidateAsync(baseEntity).ConfigureAwait(false);
            }

            var savedEntries = await SaveChangesAsync(token).ConfigureAwait(false);
            _logger.LogInformation("saved {savedEntries} in the database", savedEntries);
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
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "updating task was canceled");
            throw ex;
        }
    }
#pragma warning restore IDE0007

    public Task<DbCommand> LoadStoredProcedureAsync(string procedureName)
    {
        return LoadStoredProcedureInternalAsync(procedureName);
    }

    public async Task ExecuteStoredProcedureAsync<TEntity>(string procedureName, TEntity entity) where TEntity : BaseEntity
    {
        var command = await LoadStoredProcedureInternalAsync(procedureName).ConfigureAwait(false);
        var properties = entity.GetType().GetRuntimeProperties();

        foreach (var property in properties)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = property.Name;
            parameter.Value = property.GetValue(entity);
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync(_tokenSource.Token).ConfigureAwait(false);
    }

    public Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(() => ExecuteTransactionCoreAsync(action));
    }

    public async Task EnsureCreatedAsync()
    {
        _tokenSource ??= new CancellationTokenSource();

        var result = await Database.EnsureCreatedAsync(_tokenSource.Token).ConfigureAwait(false);
        if (result)
        {
            _logger.LogInformation("the database was successfully created");
        }

        _logger.LogError("the database already exists or an error occurred while creating database");
    }

    public async Task EnsureDeletedAsync()
    {
        _tokenSource ??= new CancellationTokenSource();

        var result = await Database.EnsureDeletedAsync(_tokenSource.Token).ConfigureAwait(false);
        if (result)
        {
            _logger.LogInformation("the database was successfully deleted");
        }

        _logger.LogError("error occurred while deleting database");
    }

    public async Task MigrateAsync()
    {
        _tokenSource ??= new CancellationTokenSource();
        await Database.MigrateAsync(_tokenSource.Token).ConfigureAwait(false);
    }

    public async Task<bool> TestConnectionAsync()
    {
        _tokenSource ??= new CancellationTokenSource();

        var openConnectionTask = Database.OpenConnectionAsync(_tokenSource.Token);
        var closeConnectionTask = Database.CloseConnectionAsync();

        await Task.WhenAll(openConnectionTask, closeConnectionTask).ConfigureAwait(false);
        return openConnectionTask.IsCompletedSuccessfully && closeConnectionTask.IsCompletedSuccessfully;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter, DateOnlyComparer>().HaveColumnType("date");
        configurationBuilder.Properties<TimeOnly>().HaveConversion<TimeOnlyConverter, TimeOnlyComparer>().HaveColumnType("time(7)");

        base.ConfigureConventions(configurationBuilder);
    }

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
        if (_tokenSource != null)
        {
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }

        if (_generator != null)
        {
            _generator.Dispose();
            _generator = null;
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}