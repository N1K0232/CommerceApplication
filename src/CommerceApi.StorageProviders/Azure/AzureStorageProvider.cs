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
        ArgumentException.ThrowIfNullOrEmpty(path);

        var token = CancellationToken;
        var blobContainerClient = await GetBlobContainerClientAsync(_azureStorageSettings.ContainerName).ConfigureAwait(false);

        await blobContainerClient.DeleteBlobIfExistsAsync(path, cancellationToken: token).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
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

        var blobContainerClient = await GetBlobContainerClientAsync(path).ConfigureAwait(false);
        var blobClient = blobContainerClient.GetBlobClient(path);

        var stream = await blobClient.OpenReadAsync(cancellationToken: token).ConfigureAwait(false);
        if (stream != null && stream.Position != 0)
        {
            stream.Position = 0;
        }

        return stream;
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var token = CancellationToken;
        var blobContainerClient = await GetBlobContainerClientAsync(_azureStorageSettings.ContainerName, true);
        var blobClient = blobContainerClient.GetBlobClient(path);

        if (!overwrite)
        {
            var blobExists = await CheckExistsAsync(path).ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        stream.Position = 0;
        var contentType = MimeUtility.GetMimeMapping(path);
        var headers = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(stream, headers, cancellationToken: token).ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync(string path) => CheckExistsAsync(path);

    private async Task<bool> CheckExistsAsync(string path)
    {
        var token = CancellationToken;

        var blobContainerClient = await GetBlobContainerClientAsync(path).ConfigureAwait(false);
        return await blobContainerClient.ExistsAsync(token).ConfigureAwait(false);
    }

    private async Task<BlobContainerClient> GetBlobContainerClientAsync(string containerName, bool createIfNotExists = false)
    {
        var token = CancellationToken;
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
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
}