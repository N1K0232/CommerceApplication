using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; init; } = null!;

    public string? LastName { get; init; }

    public DateTime DateOfBirth { get; init; }

    public int? Age { get; init; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string PhoneNumber { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? UserName { get; init; }

    public IEnumerable<string> Roles { get; init; } = null!;
}