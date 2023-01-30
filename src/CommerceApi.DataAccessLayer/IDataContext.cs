using System.Linq.Expressions;
using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer;

public interface IDataContext : IDisposable
{
    void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity;

    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

    ValueTask<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity;

    IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity;

    void Create<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Edit<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task SaveAsync();

    Task ExecuteTransactionAsync(Func<Task> action);
}