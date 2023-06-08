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
        ArgumentNullException.ThrowIfNull(path, nameof(path));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        var fileExists = CheckExists(path);
        if (!fileExists)
        {
            await CreateDirectoryAsync(path, cancellationToken).ConfigureAwait(false);
            var normalizedPath = NormalizePath(path);

            var fileMode = overwrite ? FileMode.Append : FileMode.CreateNew;
            var outputStream = new FileStream(normalizedPath, fileMode, FileAccess.Write);

            stream.Position = 0;
            await stream.CopyToAsync(outputStream, cancellationToken);

            outputStream.Close();
            await outputStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public Task<Stream?> ReadAsync(string path)
    {
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
    {
        var storageFolder = _fileSystemSettings.StorageFolder;

        var normalizedPath = Path.Combine(storageFolder, path);
        return normalizedPath;
    }

    private Task CreateDirectoryAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var normalizedPath = NormalizePath(path);
            var directoryName = Path.GetDirectoryName(normalizedPath) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(directoryName) || !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            return Task.CompletedTask;
        }
        catch (IOException ex)
        {
            return Task.FromException(ex);
        }
    }

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
}