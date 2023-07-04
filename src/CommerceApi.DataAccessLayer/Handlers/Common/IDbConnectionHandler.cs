namespace CommerceApi.DataAccessLayer.Handlers.Common;

public interface IDbConnectionHandler : IDisposable, IAsyncDisposable
{
    Task OpenAsync();

    Task CloseAsync();
}