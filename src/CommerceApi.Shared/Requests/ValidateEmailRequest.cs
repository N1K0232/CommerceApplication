namespace CommerceApi.Shared.Requests;

public class ValidateEmailRequest
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;
}