using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace CommerceApi.StorageProviders.Azure;

internal class AzureStorageProvider : IStorageProvider
{
    private AzureStorageSettings settings;
    private BlobServiceClient blobServiceClient;

    private bool disposed = false;


    public AzureStorageProvider(AzureStorageSettings settings)
    {
        this.settings = settings;
        blobServiceClient = new BlobServiceClient(settings.ConnectionString);
    }


    public async Task DeleteAsync(string path)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "the path is required");
        }

        var blobClient = await GetBlobClientAsync(settings.ContainerName).ConfigureAwait(false);
        await blobClient.DeleteBlobIfExistsAsync(path).ConfigureAwait(false);
    }

    public async Task<Stream> ReadAsync(string path)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "the path is required");
        }

        var blobContainerClient = await GetBlobClientAsync(settings.ContainerName).ConfigureAwait(false);
        var blobClient = blobContainerClient.GetBlobClient(path);

        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream).ConfigureAwait(false);

        stream.Position = 0;
        return stream;
    }

    public async Task UploadAsync(string path, Stream stream)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "the path is required");
        }

        var blobContainerClient = await GetBlobClientAsync(settings.ContainerName, true).ConfigureAwait(false);
        var blobClient = blobContainerClient.GetBlobClient(path);

        stream.Position = 0;

        var contentType = MimeUtility.GetMimeMapping(path);
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType }).ConfigureAwait(false);
    }


    private async Task<BlobContainerClient> GetBlobClientAsync(string containerName, bool createIfNotExists = false)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
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
        if (disposing && !disposed)
        {
            settings = null;
            blobServiceClient = null;

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