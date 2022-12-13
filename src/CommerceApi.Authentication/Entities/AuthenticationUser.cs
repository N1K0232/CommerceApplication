using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authentication.Entities;

public class AuthenticationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public virtual ICollection<AuthenticationUserRole> UserRoles { get; set; }
}