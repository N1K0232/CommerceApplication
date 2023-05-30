namespace CommerceApi.Client;

public interface IEmailClient : IDisposable
{
    Task SendAsync(string to, string subject, string body);
}