using CommerceApi.Client.Settings;

namespace CommerceApi.Client;

public class EmailClient : IEmailClient
{
    private EmailClientSettings settings;
    private bool disposed = false;

    public EmailClient(EmailClientSettings settings)
    {
        this.settings = settings;
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