namespace CommerceApi.TenantContext;

public class TenantContextOptions
{
    private IList<string> _availableTenants = null!;

    public virtual IList<string> AvailableTenants
    {
        get => _availableTenants;
        set => _availableTenants = value;
    }
}