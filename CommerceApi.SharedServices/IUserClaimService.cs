namespace CommerceApi.SharedServices;

public interface IUserClaimService
{
    Guid GetId();

    string GetUserName();
}