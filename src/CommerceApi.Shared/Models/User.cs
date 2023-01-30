using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class User : BaseModel
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateTime? DateOfBirth { get; init; }

    public int? Age { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Email { get; init; }

    public string? UserName { get; init; }

    public IEnumerable<string>? Roles { get; init; }
}