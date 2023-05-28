using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime RegistrationDate { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public virtual IList<ApplicationUserRole> UserRoles { get; set; }

    public virtual IList<Address> Addresses { get; set; }
}