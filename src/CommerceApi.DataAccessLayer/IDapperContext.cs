using System.Data;
using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer;

public interface IDapperContext : IDisposable
{
    Task<IEnumerable<T>> QueryAsync<T>(string query, object parameter = null, IDbTransaction transaction = null, CommandType? commandType = null) where T : BaseEntity;

    Task ExecuteAsync(string query, object parameter = null, CommandType? commandType = null);
}