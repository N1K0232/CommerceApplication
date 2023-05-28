using Microsoft.AspNetCore.Identity;

namespace CommerceApi.Authentication.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public virtual IList<ApplicationUserRole> UserRoles { get; set; }
}