﻿using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Abstractions;

public interface IDataContext : IReadOnlyDataContext
{
    void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    void Create<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Create<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    Task SaveAsync();

    Task ExecuteTransactionAsync(Func<Task> action);

    Task EnsureCreatedAsync();

    Task EnsureDeletedAsync();

    Task MigrateAsync();

    Task<bool> TestConnectionAsync();
}