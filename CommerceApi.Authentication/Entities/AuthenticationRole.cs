using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authentication.Entities;

public class AuthenticationRole : IdentityRole<Guid>
{
    public AuthenticationRole() : base()
    {
    }

    public AuthenticationRole(string roleName) : base(roleName)
    {
    }

    public virtual ICollection<AuthenticationUserRole> UserRoles { get; set; }
}