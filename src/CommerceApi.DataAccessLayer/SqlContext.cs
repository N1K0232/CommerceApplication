using System.Data;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Settings;
using CommerceApi.DataAccessLayer.TypeHandlers;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CommerceApi.DataAccessLayer;

public class SqlContext : ISqlContext
{
    private SqlConnection _activeConnection = null;
    private bool _disposed = false;

    private readonly SqlContextOptions _options;
    private readonly ILogger<SqlContext> _logger;

    static SqlContext()
    {
        SqlMapper.AddTypeHandler(new StringArrayTypeHandler());
    }

    public SqlContext(SqlContextOptions options, ILogger<SqlContext> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<IEnumerable<T>> GetDataAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync<T>(sql, param, transaction, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TFourth : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<T> GetObjectAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TFourth : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.QueryAsync(sql, map, param, transaction, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<T> GetSingleValueAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var value = await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType).ConfigureAwait(false);
        return value;
    }

    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);

        var result = await connection.ExecuteAsync(sql, param, transaction, commandType: commandType).ConfigureAwait(false);
        return result;
    }

    public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var transaction = connection.BeginTransaction(isolationLevel);
        return transaction;
    }

    private async Task<IDbConnection> GetConnectionAsync()
    {
        _activeConnection ??= new SqlConnection(_options.ConnectionString);

        if (_activeConnection.State is ConnectionState.Closed)
        {
            await _activeConnection.OpenAsync().ConfigureAwait(false);
        }

        return _activeConnection;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        var canDispose = disposing && !_disposed;
        if (canDispose)
        {
            if (_activeConnection.State is ConnectionState.Open)
            {
                _activeConnection.Close();
            }

            _activeConnection.Dispose();
            _activeConnection = null;

            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}