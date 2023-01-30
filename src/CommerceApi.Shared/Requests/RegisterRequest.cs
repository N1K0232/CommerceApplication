using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class RegisterRequest : BaseRequestModel
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}