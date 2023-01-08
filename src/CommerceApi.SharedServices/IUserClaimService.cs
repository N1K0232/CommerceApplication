namespace CommerceApi.SharedServices;

public interface IUserClaimService
{
    Guid GetApplicationId();
    Guid GetId();

    string GetUserName();
}