using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace CommerceApi.Client;

public class EmailClient : IEmailClient
{
    private SmtpClient client = null;
    private MimeMessage message = null;

    private string apiKey = null;
    private string host = null;
    private int port = 0;

    private bool disposed = false;

    private readonly IConfiguration configuration;
    private readonly ILogger<EmailClient> logger;

    public EmailClient(IConfiguration configuration, ILogger<EmailClient> logger)
    {
        this.configuration = configuration;
        this.logger = logger;

        Initialize();
    }

    public async Task SendAsync(string to, string subject, string body)
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

            var section = configuration.GetSection("EmailClientSettings");

            var fromEmailAddress = section["FromEmailAddress"];
            var fromMailboxAddress = MailboxAddress.Parse(fromEmailAddress);
            message.From.Add(fromMailboxAddress);

            var destinationMailboxAddress = MailboxAddress.Parse(to);
            message.To.Add(destinationMailboxAddress);

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            client = new SmtpClient();
            await client.ConnectAsync(host, port);

            var userName = section["EmailUserName"];
            var password = section["EmailPassword"];

            await client.AuthenticateAsync(userName, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "couldn't send image");
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

            apiKey = null;
            host = null;
            port = 0;

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

    private void Initialize()
    {
        var section = configuration.GetSection("EmailClientSettings");

        apiKey = section["ApiKey"];
        host = section["Host"];
        port = section.GetValue<int>("Port");
    }
}