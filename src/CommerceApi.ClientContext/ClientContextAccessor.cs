namespace CommerceApi.ClientContext;

internal class ClientContextAccessor : IClientContextAccessor
{
    private static readonly AsyncLocal<IClientContext> _clientContext = new();

    public IClientContext ClientContext
    {
        get => _clientContext.Value;
        set => _clientContext.Value = value;
    }
}