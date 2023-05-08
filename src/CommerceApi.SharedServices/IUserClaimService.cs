using System.Security.Claims;

namespace CommerceApi.SharedServices;

public interface IUserClaimService
{
    string GetApplicationId();

    Guid GetId();

    Guid GetTenantId();

    string GetUserName();

    ClaimsIdentity GetIdentity();
}