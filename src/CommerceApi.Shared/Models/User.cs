using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; init; } = null!;

    public string? LastName { get; init; }

    public DateTime DateOfBirth { get; init; }

    public int? Age { get; init; }

    public string PhoneNumber { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? UserName { get; init; }

    public IEnumerable<Address> Addresses { get; set; } = null!;

    public IEnumerable<string> Roles { get; init; } = null!;
}