using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Requests;

public class RegisterRequest : BaseRequestModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
}