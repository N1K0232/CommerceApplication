using CommerceApi.StorageProviders.Abstractions;

namespace CommerceApi.StorageProviders.FileSystem;

public class FileSystemStorageProvider : StorageProvider, IStorageProvider
{
    private FileSystemSettings settings;

    public FileSystemStorageProvider(FileSystemSettings settings)
    {
        this.settings = settings;
    }

    public override async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        ThrowIfDisposed();

        var fullPath = await GetFullPathAsync(path).ConfigureAwait(false);
        if (!File.Exists(fullPath))
        {
            var directoryName = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var outputStream = !overwrite ?
                new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write) :
                new FileStream(fullPath, FileMode.Append, FileAccess.Write);

            stream.Position = 0;
            await stream.CopyToAsync(outputStream).ConfigureAwait(false);

            outputStream.Close();
            await outputStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public override async Task<Stream?> ReadAsync(string path)
    {
        var fullPath = await GetFullPathAsync(path).ConfigureAwait(false);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        var stream = File.OpenRead(fullPath);
        return stream;
    }

    public override async Task DeleteAsync(string path)
    {
        var fullPath = await GetFullPathAsync(path).ConfigureAwait(false);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            settings = null!;
        }

        base.Dispose(disposing);
    }

    private async Task<string> GetFullPathAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path), "the path is required");
        }

        var normalizedPath = await NormalizePathAsync(path).ConfigureAwait(false);

        var fullPath = Path.Combine(settings.StorageFolder, normalizedPath);
        return fullPath;
    }
}