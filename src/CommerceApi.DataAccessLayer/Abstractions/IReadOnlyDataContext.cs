using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Abstractions;

public interface IReadOnlyDataContext
{
    ValueTask<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity;

    IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity;
}