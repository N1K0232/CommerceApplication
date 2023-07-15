using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Abstractions;

public interface IReadOnlyDataContext
{
    ValueTask<TEntity> GetAsync<TEntity>(params object[] keyValues) where TEntity : BaseEntity;

    IQueryable<TEntity> Get<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity;
}