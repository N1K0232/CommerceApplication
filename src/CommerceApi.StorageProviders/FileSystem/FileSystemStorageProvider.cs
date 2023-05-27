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

        var normalizedPath = await NormalizePathAsync(path).ConfigureAwait(false);
        if (!File.Exists(normalizedPath))
        {
            var directoryName = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var outputStream = await GetOutputStreamAsync(path, overwrite).ConfigureAwait(false);

            stream.Position = 0;
            await stream.CopyToAsync(outputStream).ConfigureAwait(false);

            outputStream.Close();
            await outputStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public override async Task<Stream?> ReadAsync(string path)
    {
        var normalizedPath = await NormalizePathAsync(path).ConfigureAwait(false);
        if (!File.Exists(normalizedPath))
        {
            return null;
        }

        var stream = File.OpenRead(normalizedPath) ?? Stream.Null;
        return stream;
    }

    public override async Task DeleteAsync(string path)
    {
        var normalizedPath = await NormalizePathAsync(path).ConfigureAwait(false);
        if (File.Exists(normalizedPath))
        {
            File.Delete(normalizedPath);
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

    protected override async Task<string> NormalizePathAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path), "the path is required");
        }

        var normalizedPath = await base.NormalizePathAsync(path).ConfigureAwait(false);

        var fullPath = Path.Combine(settings.StorageFolder, normalizedPath);
        return fullPath;
    }

    private Task<Stream> GetOutputStreamAsync(string path, bool overwrite)
    {
        var stream = !overwrite ?
                new FileStream(path, FileMode.CreateNew, FileAccess.Write) :
                new FileStream(path, FileMode.Append, FileAccess.Write);

        return Task.FromResult<Stream>(stream);
    }
}