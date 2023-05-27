﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography;
using CommerceApi.DataAccessLayer.Comparers;
using CommerceApi.DataAccessLayer.Converters;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
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
    private IDbContextTransaction transaction = null;

    private RandomNumberGenerator generator = null;

    protected override void OnSavedChanges(object sender, SaveChangesEventArgs e) => base.OnSavedChanges(sender, e);

    protected override void OnSavingChanges(object sender, SavingChangesEventArgs e) => base.OnSavingChanges(sender, e);

    protected override void OnSaveChangesFailed(object sender, SaveChangesFailedEventArgs e) => base.OnSaveChangesFailed(sender, e);

    private async Task ExecuteTransactionCoreAsync(Func<Task> action)
    {
        tokenSource ??= new CancellationTokenSource();
        var token = tokenSource.Token;

        transaction = await Database.BeginTransactionAsync(token).ConfigureAwait(false);
        await action.Invoke().ConfigureAwait(false);
        await transaction.CommitAsync(token).ConfigureAwait(false);
    }

    private Task ValidateAsync(BaseEntity entity)
    {
        try
        {
            validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext, true);

            return Task.CompletedTask;
        }
        catch (ValidationException ex)
        {
            return Task.FromException(ex);
        }
    }

    private string GenerateSecurityStamp()
    {
        generator = RandomNumberGenerator.Create();
        var bytes = new byte[50];

        generator.GetBytes(bytes);

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

    partial void ConfigureConventionsCore(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter, DateOnlyComparer>().HaveColumnType("date");
        configurationBuilder.Properties<TimeOnly>().HaveConversion<TimeOnlyConverter, TimeOnlyComparer>().HaveColumnType("time(7)");
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
        var builder = modelBuilder.Entity<TEntity>();
        builder.HasQueryFilter(e => !e.IsDeleted && e.DeletedDate == null && e.DeletedTime == null);
    }

    private void SetQueryFilterOnTenantEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        var tenantId = claimService.GetTenantId();
        var builder = modelBuilder.Entity<TEntity>();
        builder.HasQueryFilter(e => e.TenantId == tenantId);
    }
}