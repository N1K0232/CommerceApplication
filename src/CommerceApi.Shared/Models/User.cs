using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class User : BaseModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string UserName { get; set; }
}