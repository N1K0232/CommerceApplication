using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Supplier : BaseModel
{
    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string City { get; set; }
}