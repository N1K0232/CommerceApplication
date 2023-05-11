namespace CommerceApi.ClientContext;

public interface IClientContextAccessor
{
    IClientContext ClientContext { get; internal set; }
}