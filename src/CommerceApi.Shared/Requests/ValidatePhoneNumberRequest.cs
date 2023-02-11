namespace CommerceApi.Shared.Requests;

public class ValidatePhoneNumberRequest
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
}