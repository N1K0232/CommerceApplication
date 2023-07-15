using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Address : BaseObject
{
    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;
}