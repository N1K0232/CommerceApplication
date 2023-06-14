namespace CommerceApi.TenantContext;

public interface ITenantContextAccessor
{
    ITenantContext? TenantContext { get; internal set; }
}