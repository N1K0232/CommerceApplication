using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace CommerceApi.StorageProviders.Azure;

internal class AzureStorageProvider : IStorageProvider
{
    private const string DefaultContainerName = "attachments";

    private AzureStorageSettings settings;
    private BlobServiceClient blobServiceClient;

    private bool disposed = false;


    public AzureStorageProvider(AzureStorageSettings settings)
    {
        this.settings = settings;
        blobServiceClient = new BlobServiceClient(settings.ConnectionString);
    }

    public async Task SaveAsync(string? path, Stream stream, bool overwrite = false)
    {
        ThrowIfDisposed();

        var blobClient = await GetBlobClientAsync(path, true).ConfigureAwait(false);
        if (!overwrite)
        {
            var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        stream.Position = 0;

        var contentType = MimeUtility.GetMimeMapping(path);
        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(stream, headers).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
        ThrowIfDisposed();

        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync().ConfigureAwait(false);
        stream.Position = 0;

        return stream;
    }

    public async Task DeleteAsync(string path)
    {
        ThrowIfDisposed();

        var properties = ExtractBlobContainerName(path);
        if (properties != null)
        {
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(properties.ContainerName);
            await blobContainerClient.DeleteBlobIfExistsAsync(properties.BlobName).ConfigureAwait(false);
        }
    }

    private async Task<BlobClient> GetBlobClientAsync(string? path, bool createIfNotExists = false)
    {
        var properties = ExtractBlobContainerName(path);
        var containerName = properties?.ContainerName ?? settings.ContainerName?.ToLowerInvariant() ?? DefaultContainerName;

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
        }

        var blobClient = blobContainerClient.GetBlobClient(properties?.BlobName ?? string.Empty);
        return blobClient;
    }

    private AzureStorageProperties? ExtractBlobContainerName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fixedPath = path.Replace(@"\", "/");
        var root = Path.GetPathRoot(fixedPath);

        var fileName = fixedPath[(root ?? string.Empty).Length..];
        var parts = fileName.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        var properties = new AzureStorageProperties(containerName, blobName);
        return properties;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            settings = null!;
            blobServiceClient = null!;

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}