namespace CommerceApi.StorageProviders;

public interface IStorageProvider : IDisposable
{
    Task DeleteAsync(string path);

    Task<Stream> ReadAsync(string path);

    Task UploadAsync(string path, Stream stream);
}