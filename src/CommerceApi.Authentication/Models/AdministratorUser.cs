namespace CommerceApi.Authentication.Models;

public class AdministratorUser
{
    public string FirstName { get; init; }

    public string LastName { get; init; }

    public DateTime DateOfBirth { get; init; }

    public string PhoneNumber { get; init; }

    public string Email { get; init; }

    public string UserName { get; init; }

    public string Password { get; init; }
}