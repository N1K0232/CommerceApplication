using CommerceApi.StorageProviders.Abstractions;

namespace CommerceApi.StorageProviders.Extensions;

public static class StorageProviderExtensions
{
    public static async Task SaveAsync(this IStorageProvider storageProvider, string path, byte[] content, bool overwrite = false)
    {
        if (content is null || content.Length == 0)
        {
            throw new ArgumentNullException(nameof(content), "the content is required");
        }

        var memoryStream = new MemoryStream(content);
        await storageProvider.SaveAsync(path, memoryStream, overwrite).ConfigureAwait(false);
        await memoryStream.DisposeAsync().ConfigureAwait(false);
    }

    public static async Task<byte[]?> ReadAsync(this IStorageProvider storageProvider, string path)
    {
        var stream = await storageProvider.ReadAsync(path).ConfigureAwait(false);
        if (stream is null)
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

        var content = memoryStream.ToArray();
        await memoryStream.DisposeAsync().ConfigureAwait(false);
        await stream.DisposeAsync().ConfigureAwait(false);

        return content;
    }
}