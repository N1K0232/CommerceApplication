using System.Data;
using CommerceApi.DataAccessLayer.Handlers.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CommerceApi.DataAccessLayer.Handlers;

public class SqlConnectionHandler : IDbConnectionHandler
{
    private SqlConnection _connection;
    private CancellationTokenSource _tokenSource;

    private bool _disposed = false;

    public SqlConnectionHandler(IConfiguration configuration)
    {
        Initialize(configuration);
    }

    public async Task OpenAsync()
    {
        ThrowIfDisposed();
        await _connection.OpenAsync(_tokenSource.Token).ConfigureAwait(false);
    }

    public async Task CloseAsync()
    {
        ThrowIfDisposed();
        await _connection.CloseAsync().ConfigureAwait(false);
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

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !_disposed)
        {
            if (_connection != null)
            {
                if (_connection.State is ConnectionState.Open)
                {
                    await _connection.CloseAsync().ConfigureAwait(false);
                }

                await _connection.DisposeAsync().ConfigureAwait(false);
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

    private void Initialize(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlConnection");
        _connection = new SqlConnection(connectionString);
        _tokenSource = new CancellationTokenSource();
    }
}