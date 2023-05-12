namespace CommerceApi.StorageProviders.Extensions;

public static class StorageProviderExtensions
{
    public static async Task<byte[]?> ReadAsByteAsync(this IStorageProvider storageProvider, string path)
    {
        using var input = await storageProvider.ReadAsync(path).ConfigureAwait(false);
        if (input is null)
        {
            return null;
        }

        var content = new Memory<byte>();
        await input.ReadAsync(content).ConfigureAwait(false);

        var output = content.ToArray();
        return output;
    }

    public static async Task<string?> ReadAsStringAsync(this IStorageProvider storageProvider, string path)
    {
        using var input = await storageProvider.ReadAsync(path).ConfigureAwait(false);
        if (input is null)
        {
            return null;
        }

        using var reader = new StreamReader(input);

        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return content;
    }

    public static async Task UploadAsync(this IStorageProvider storageProvider, string path, byte[] content, bool overwrite = false)
    {
        var stream = new MemoryStream(content);
        await storageProvider.SaveAsync(path, stream, overwrite).ConfigureAwait(false);
    }
}