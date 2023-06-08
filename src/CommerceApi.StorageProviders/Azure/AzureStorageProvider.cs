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

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        var cancellationToken = CancellationToken;
        var blobClient = await GetBlobClientAsync(path, true).ConfigureAwait(false);
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

        await blobClient.UploadAsync(stream, headers, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
        var cancellationToken = CancellationToken;

        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        var blobExists = await CheckExistsAsync(path).ConfigureAwait(false);
        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return stream;
    }

    public async Task DeleteAsync(string path)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = GetBlobContainerClient(containerName);

        var cancellationToken = CancellationToken;
        await blobContainerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync(string path) => CheckExistsAsync(path);

    private async Task<bool> CheckExistsAsync(string path)
    {
        var cancellationToken = CancellationToken;

        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        return await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = GetBlobContainerClient(containerName);

        if (createIfNotExists)
        {
            var cancellationToken = CancellationToken;
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var blobClient = blobContainerClient.GetBlobClient(blobName);
        return blobClient;
    }

    private BlobContainerClient GetBlobContainerClient(string containerName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        return blobContainerClient;
    }

    private (string ContainerName, string BlobName) ExtractContainerBlobName(string? path)
    {
        var normalizedPath = path?.Replace(@"\", "/") ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_azureStorageSettings.ContainerName))
        {
            return (_azureStorageSettings.ContainerName.ToLowerInvariant(), normalizedPath);
        }

        var root = Path.GetPathRoot(normalizedPath);
        var fileName = normalizedPath[(root ?? string.Empty).Length..];

        var parts = fileName.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        return (containerName, blobName);
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