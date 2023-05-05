using CommerceApi.Authentication.Enums;
using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authentication.Entities;

public class AuthenticationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public DateTime RegistrationDate { get; set; }

    public UserStatus Status { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public virtual ICollection<AuthenticationUserRole> UserRoles { get; set; }
}