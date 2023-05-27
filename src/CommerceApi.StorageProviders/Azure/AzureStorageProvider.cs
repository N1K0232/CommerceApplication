using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommerceApi.StorageProviders.Abstractions;
using MimeMapping;

namespace CommerceApi.StorageProviders.Azure;

public class AzureStorageProvider : StorageProvider, IStorageProvider
{
    private AzureStorageSettings azureStorageSettings;
    private BlobServiceClient blobServiceClient;

    public AzureStorageProvider(AzureStorageSettings azureStorageSettings)
    {
        this.azureStorageSettings = azureStorageSettings;
        blobServiceClient = new BlobServiceClient(azureStorageSettings.ConnectionString);
    }

    public override async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);
        if (!overwrite)
        {
            var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        stream.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = MimeUtility.GetMimeMapping(path) };
        await blobClient.UploadAsync(stream, headers).ConfigureAwait(false);
    }

    public override async Task<Stream?> ReadAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path).ConfigureAwait(false);

        var blobExists = await blobClient.ExistsAsync().ConfigureAwait(false);
        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync();
        return stream;
    }

    public override async Task DeleteAsync(string path)
    {
        var properties = await ExtractContainerBlobClientAsync(path).ConfigureAwait(false);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(properties.ContainerName);

        await blobContainerClient.DeleteBlobIfExistsAsync(properties.BlobName).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(string? path, bool createIfNotExists = false)
    {
        var properties = await ExtractContainerBlobClientAsync(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(properties.ContainerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        var blobClient = blobContainerClient.GetBlobClient(properties.BlobName);
        return blobClient;
    }

    private async Task<AzureStorageProperties> ExtractContainerBlobClientAsync(string? path)
    {
        var normalizedPath = await NormalizePathAsync(path).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(azureStorageSettings.ContainerName))
        {
            return new AzureStorageProperties { ContainerName = azureStorageSettings.ContainerName, BlobName = normalizedPath };
        }

        var root = Path.GetPathRoot(normalizedPath);

        var fileName = normalizedPath[(root ?? string.Empty).Length..];
        var parts = fileName.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        var azureStorageProperties = new AzureStorageProperties { ContainerName = containerName, BlobName = blobName };
        return azureStorageProperties;
    }

    protected override Task<string> NormalizePathAsync(string? path)
    {
        var normalizedPath = path?.Replace(@"\", "/") ?? string.Empty;
        return base.NormalizePathAsync(normalizedPath);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            azureStorageSettings = null!;
            blobServiceClient = null!;
        }

        base.Dispose(disposing);
    }
}