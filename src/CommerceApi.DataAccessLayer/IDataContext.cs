using System.Linq.Expressions;
using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer;

public interface IDataContext : IDisposable
{
    void Delete<T>(T entity) where T : BaseEntity;

    void Delete<T>(IEnumerable<T> entities) where T : BaseEntity;

    void Edit<T>(T entity) where T : BaseEntity;

    void Edit<T>(IEnumerable<T> entities) where T : BaseEntity;

    Task<bool> ExistsAsync<T>(Guid id) where T : BaseEntity;

    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

    Task<T> GetAsync<T>(Guid id) where T : BaseEntity;

    IQueryable<T> GetData<T>(bool ignoreAutoIncludes = true, bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity;

    void Create<T>(T entity) where T : BaseEntity;

    void Create<T>(IEnumerable<T> entities) where T : BaseEntity;

    Task<int> SaveAsync();

    Task ExecuteTransactionAsync();
}