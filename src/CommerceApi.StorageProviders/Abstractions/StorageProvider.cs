namespace CommerceApi.StorageProviders.Abstractions;

public abstract class StorageProvider : IStorageProvider
{
    private bool disposed = false;

    protected StorageProvider()
    {
    }

    public abstract Task SaveAsync(string path, Stream stream, bool overwrite = false);

    public abstract Task<Stream?> ReadAsync(string path);

    public abstract Task DeleteAsync(string path);

    protected virtual Task<string> NormalizePathAsync(string? path)
    {
        var normalizedPath = path?.Normalize().ToLowerInvariant() ?? string.Empty;
        return Task.FromResult(normalizedPath.Trim());
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        var canDispose = disposing && !disposed;
        if (canDispose)
        {
            disposed = true;
        }
    }

    protected void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}