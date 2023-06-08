using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace CommerceApi.Client;

public class EmailClient : IEmailClient
{
    private SmtpClient _client;
    private MimeMessage _message;

    private CancellationTokenSource _tokenSource;

    private string _apiKey;
    private string _host;
    private int _port = 0;

    private bool _disposed = false;

    private readonly IConfigurationSection _emailClientSettings;
    private readonly ILogger<EmailClient> _logger;

    public EmailClient(IConfiguration configuration, ILogger<EmailClient> logger)
    {
        _emailClientSettings = configuration.GetSection("EmailClientSettings");
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        ThrowIfDisposed();

        _apiKey = _emailClientSettings["ApiKey"];
        _host = _emailClientSettings["Host"];
        _port = _emailClientSettings.GetValue<int>("Port");

        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogError("the destination is required", to);
            throw new ArgumentNullException(nameof(to), "the destination is required");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            _logger.LogError("the subject is required", subject);
            throw new ArgumentNullException(nameof(subject), "the subject is required");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogError("the body is required", body);
            throw new ArgumentNullException(nameof(body), "the body is required");
        }

        try
        {
            _message = new MimeMessage();

            var fromMailboxAddress = GetAddress("FromEmailAddress");
            _message.From.Add(fromMailboxAddress);

            var destinationMailboxAddress = MailboxAddress.Parse(to);
            _message.To.Add(destinationMailboxAddress);

            _message.Subject = subject;
            _message.Body = new TextPart(TextFormat.Html) { Text = body };

            _client = new SmtpClient();
            await _client.ConnectAsync(_host, _port);

            var userName = _emailClientSettings["EmailUserName"];
            var password = _emailClientSettings["EmailPassword"];

            await _client.AuthenticateAsync(userName, password);
            await _client.SendAsync(_message);
            await _client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "couldn't send image");
        }
    }

    private MailboxAddress GetAddress(string key)
    {
        var address = _emailClientSettings[key];

        var mailboxAddress = MailboxAddress.Parse(address);
        return mailboxAddress;
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
            _client.Dispose();
            _client = null;

            _message.Dispose();
            _message = null;

            _apiKey = null;
            _host = null;
            _port = 0;

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