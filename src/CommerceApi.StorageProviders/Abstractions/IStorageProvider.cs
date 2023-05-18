namespace CommerceApi.StorageProviders.Abstractions;

public interface IStorageProvider : IDisposable
{
    Task SaveAsync(string path, Stream stream, bool overwrite = false);

    Task<Stream?> ReadAsync(string path);

    Task DeleteAsync(string path);
}