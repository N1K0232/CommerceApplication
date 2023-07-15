namespace CommerceApi.TenantContext;

internal class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<ITenantContext?> _tenantContext = new();

    public ITenantContext? TenantContext
    {
        get => _tenantContext.Value;
        set => _tenantContext.Value = value;
    }
}