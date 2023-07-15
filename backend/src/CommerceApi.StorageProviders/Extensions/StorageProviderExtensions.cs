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
    }

    public static async Task<byte[]?> ReadBytesAsync(this IStorageProvider storageProvider, string path)
    {
        using var stream = await storageProvider.ReadAsync(path).ConfigureAwait(false);
        if (stream is null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

        var content = memoryStream.ToArray();
        return content;
    }

    public static async Task<string?> ReadStringAsync(this IStorageProvider storageProvider, string path)
    {
        using var stream = await storageProvider.ReadAsync(path).ConfigureAwait(false);
        if (stream is null)
        {
            return null;
        }

        using var streamReader = new StreamReader(stream);

        var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        return content;
    }
}