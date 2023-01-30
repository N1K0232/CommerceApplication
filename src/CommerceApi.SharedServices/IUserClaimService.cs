namespace CommerceApi.SharedServices;

public interface IUserClaimService
{
    string GetApplicationId();

    Guid GetId();

    string GetUserName();
}