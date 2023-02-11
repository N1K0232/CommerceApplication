using CommerceApi.Client.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace CommerceApi.Client;

#pragma warning disable IDE0007 //use implicit type
public class EmailClient : IEmailClient
{
    private SmtpClient client;
    private MimeMessage message;

    private EmailClientSettings settings;

    private readonly IConfiguration configuration;
    private readonly ILogger<EmailClient> logger;

    private bool disposed = false;

    public EmailClient(EmailClientSettings settings, IConfiguration configuration, ILogger<EmailClient> logger)
    {
        client = null;
        message = null;

        this.settings = settings;

        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(to))
        {
            logger.LogError("the destination is required", to);
            throw new ArgumentNullException(nameof(to), "the destination is required");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            logger.LogError("the subject is required", subject);
            throw new ArgumentNullException(nameof(subject), "the subject is required");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            logger.LogError("the body is required", body);
            throw new ArgumentNullException(nameof(body), "the body is required");
        }

        try
        {
            message = new MimeMessage();

            string emailAddress = configuration.GetValue<string>("EmailAddress");
            message.From.Add(MailboxAddress.Parse(emailAddress));

            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            client = new SmtpClient();
            await client.ConnectAsync(settings.Host, settings.Port);

            string userName = configuration.GetValue<string>("EmailUserName");
            string password = configuration.GetValue<string>("EmailPassword");
            await client.AuthenticateAsync(userName, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "couldn't send email");
            await Task.FromException(ex);
        }
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
            client.Dispose();
            client = null;

            message.Dispose();
            message = null;

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
#pragma warning restore IDE0007 //use implicit type