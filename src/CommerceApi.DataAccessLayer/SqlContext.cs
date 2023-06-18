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
    private SqlConnection _connection = null;
    private CancellationTokenSource _tokenSource = null;

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

    ~SqlContext()
    {
        Dispose(disposing: false);
    }

    public async Task<IEnumerable<T>> GetDataAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> GetDataAsync<TFirst, TSecond, TThrid, TReturn>(string sql, Func<TFirst, TSecond, TThrid, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThrid : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
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
        var commandTimeout = _options.CommandTimeout;
        return await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<T> GetObjectAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
        where T : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TReturn : class
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;

        var result = await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<TReturn> GetObjectAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, CommandType? commandType = null, string splitOn = "Id")
        where TFirst : class
        where TSecond : class
        where TThird : class
        where TReturn : class
    {
        ThrowIfDisposed();

        _logger.LogInformation("executes the query and gets the single object found");
        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;

        var result = await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
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

        _logger.LogInformation("executes the query and gets the single object found");
        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;

        var result = await connection.QueryAsync(sql, map, param, transaction, commandTimeout: commandTimeout, splitOn: splitOn, commandType: commandType).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<T> GetSingleValueAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CommandType? commandType = null)
    {
        ThrowIfDisposed();

        _logger.LogInformation("executes a query such as stored procedures");

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        var commandTimeout = _options.CommandTimeout;
        return await connection.ExecuteAsync(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        ThrowIfDisposed();

        var connection = await GetConnectionAsync().ConfigureAwait(false);
        return connection.BeginTransaction(isolationLevel);
    }

    public async Task<bool> TestConnectionAsync()
    {
        var connectionString = _options.ConnectionString;
        _connection ??= new SqlConnection(connectionString);
        _tokenSource ??= new CancellationTokenSource();

        try
        {
            await _connection.OpenAsync(_tokenSource.Token);
            await _connection.CloseAsync();

            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Unable to connect to database");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Unable to connect to database");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "task was canceled unexpectedly");
            return false;
        }
    }

    private async Task<IDbConnection> GetConnectionAsync()
    {
        var connectionString = _options.ConnectionString;
        _connection ??= new SqlConnection(connectionString);
        _tokenSource ??= new CancellationTokenSource();

        if (_connection.State is ConnectionState.Closed)
        {
            var token = _tokenSource.Token;
            await _connection.OpenAsync(token).ConfigureAwait(false);
        }

        return _connection;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            if (_connection != null)
            {
                if (_connection.State is ConnectionState.Open)
                {
                    _connection.Close();
                }

                _connection.Dispose();
                _connection = null;
            }

            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }

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