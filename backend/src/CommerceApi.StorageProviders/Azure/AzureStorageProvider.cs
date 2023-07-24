using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommerceApi.StorageProviders.Abstractions;
using MimeMapping;

namespace CommerceApi.StorageProviders.Azure;

public class AzureStorageProvider : IStorageProvider
{
    private AzureStorageSettings _azureStorageSettings;
    private BlobServiceClient _blobServiceClient;

    private CancellationTokenSource? _tokenSource;

    private bool _disposed = false;

    public AzureStorageProvider(AzureStorageSettings azureStorageSettings)
    {
        _azureStorageSettings = azureStorageSettings;
        _blobServiceClient = new BlobServiceClient(azureStorageSettings.ConnectionString);
    }

    ~AzureStorageProvider()
    {
        Dispose(disposing: false);
    }

    private CancellationToken CancellationToken
    {
        get
        {
            _tokenSource ??= new CancellationTokenSource();
            return _tokenSource.Token;
        }
    }

    public async Task DeleteAsync(string path)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(path);

        var token = CancellationToken;

        var blobContainerClient = await GetBlobContainerClientAsync().ConfigureAwait(false);
        await blobContainerClient.DeleteBlobIfExistsAsync(path, cancellationToken: token).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var token = CancellationToken;
        var blobExists = await CheckExistsAsync(path).ConfigureAwait(false);
        if (!blobExists)
        {
            return null;
        }

        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        var stream = await blobClient.OpenReadAsync(cancellationToken: token).ConfigureAwait(false);

        if (stream != null && stream.Position != 0)
        {
            stream.Position = 0;
        }

        return stream;
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var token = CancellationToken;
        var blobClient = await GetBlobClientAsync(path, true);

        if (!overwrite)
        {
            var blobExists = await CheckExistsAsync(path).ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        stream.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = MimeUtility.GetMimeMapping(path) };
        await blobClient.UploadAsync(stream, headers, cancellationToken: token).ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync(string path)
    {
        ThrowIfDisposed();
        return CheckExistsAsync(path);
    }

    private async Task<bool> CheckExistsAsync(string path)
    {
        var token = CancellationToken;

        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        return await blobClient.ExistsAsync(token).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var blobContainerClient = await GetBlobContainerClientAsync(createIfNotExists).ConfigureAwait(false);
        return blobContainerClient.GetBlobClient(path);
    }

    private async Task<BlobContainerClient> GetBlobContainerClientAsync(bool createIfNotExists = false)
    {
        var token = CancellationToken;
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_azureStorageSettings.ContainerName);
        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: token).ConfigureAwait(false);
        }

        return blobContainerClient;
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
            _azureStorageSettings = null!;
            _blobServiceClient = null!;

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