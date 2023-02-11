namespace CommerceApi.Client;

public interface IEmailClient : IDisposable
{
    Task SendEmailAsync(string to, string subject, string body);
}