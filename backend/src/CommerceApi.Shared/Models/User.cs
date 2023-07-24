using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? UserName { get; set; }

    public IEnumerable<Address> Addresses { get; set; } = null!;

    public IEnumerable<string> Roles { get; set; } = null!;
}