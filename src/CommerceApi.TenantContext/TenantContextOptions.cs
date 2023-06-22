namespace CommerceApi.TenantContext;

public class TenantContextOptions
{
    private IList<string>? _availableTenants;

    public virtual IList<string> AvailableTenants
    {
        get => _availableTenants ?? new List<string>();
        set => _availableTenants = value ?? new List<string>();
    }
}