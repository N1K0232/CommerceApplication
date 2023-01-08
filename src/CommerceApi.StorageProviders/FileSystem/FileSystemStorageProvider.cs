namespace CommerceApi.StorageProviders.FileSystem;

internal class FileSystemStorageProvider : IStorageProvider
{
    private FileSystemStorageSettings settings;
    private bool disposed = false;


    public FileSystemStorageProvider(FileSystemStorageSettings settings)
    {
        this.settings = settings;
    }


    public Task DeleteAsync(string path)
    {
        try
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    public Task<Stream> ReadAsync(string path)
    {
        var fullPath = GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    public async Task UploadAsync(string path, Stream stream)
    {
        try
        {
            var fullPath = GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                var directoryName = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                using var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

                stream.Position = 0;
                await stream.CopyToAsync(outputStream);

                outputStream.Close();
            }
        }
        catch (Exception ex)
        {
            await Task.FromException(ex);
        }
    }

    private string GetFullPath(string path) => Path.Combine(settings.StorageFolder, path);


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