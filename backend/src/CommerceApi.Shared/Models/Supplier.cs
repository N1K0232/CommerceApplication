using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Supplier : BaseObject
{
    public string CompanyName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string City { get; set; } = null!;
}