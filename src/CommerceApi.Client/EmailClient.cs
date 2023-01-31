using CommerceApi.Client.Settings;

namespace CommerceApi.Client;

public class EmailClient : IEmailClient
{
    private bool disposed = false;

    public EmailClient(EmailClientSettings settings)
    {
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
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