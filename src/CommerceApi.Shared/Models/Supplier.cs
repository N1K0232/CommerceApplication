using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Supplier : BaseModel
{
    public string? CompanyName { get; init; }

    public string? ContactName { get; init; }

    public string? City { get; init; }
}