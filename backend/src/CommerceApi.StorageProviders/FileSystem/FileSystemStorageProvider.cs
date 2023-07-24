using CommerceApi.StorageProviders.Abstractions;

namespace CommerceApi.StorageProviders.FileSystem;

public class FileSystemStorageProvider : IStorageProvider
{
    private FileSystemSettings _fileSystemSettings;
    private CancellationTokenSource? _tokenSource;

    private bool _disposed = false;

    public FileSystemStorageProvider(FileSystemSettings fileSystemSettings)
    {
        _fileSystemSettings = fileSystemSettings;
    }

    private CancellationToken CancellationToken
    {
        get
        {
            _tokenSource ??= new CancellationTokenSource();
            return _tokenSource.Token;
        }
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(path, nameof(path));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        var fileExists = CheckExists(path);
        if (!fileExists)
        {
            var normalizedPath = NormalizePath(path);
            var directoryName = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
            }

            var fileMode = overwrite ? FileMode.Append : FileMode.CreateNew;
            var outputStream = new FileStream(normalizedPath, fileMode, FileAccess.Write);

            stream.Position = 0;
            await stream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);

            outputStream.Close();
            await outputStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public Task<Stream?> ReadAsync(string path)
    {
        ThrowIfDisposed();

        var fileExists = CheckExists(path);
        if (!fileExists)
        {
            return Task.FromResult<Stream?>(null);
        }

        var normalizedPath = NormalizePath(path);

        var stream = File.OpenRead(normalizedPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string path)
    {
        ThrowIfDisposed();

        var fileExists = CheckExists(path);
        if (fileExists)
        {
            var normalizedPath = NormalizePath(path);
            File.Delete(normalizedPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path)
    {
        ThrowIfDisposed();

        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        var fileExists = CheckExists(path);
        return Task.FromResult(fileExists);
    }

    private bool CheckExists(string path)
    {
        var normalizedPath = NormalizePath(path);
        return File.Exists(normalizedPath);
    }

    private string NormalizePath(string path)
        => Path.Combine(_fileSystemSettings.StorageFolder, path);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _fileSystemSettings = null!;
            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}