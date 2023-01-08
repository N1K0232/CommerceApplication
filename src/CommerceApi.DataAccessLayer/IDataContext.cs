using System.Linq.Expressions;
using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer;

public interface IDataContext : IDisposable
{
    Task DeleteAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity;

    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

    Task<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity;

    IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity;

    Task CreateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task UpdateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task ExecuteTransactionAsync(Func<Task> action);
}