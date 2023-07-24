namespace CommerceApi.Shared.Models.Requests;

public class SaveAddressRequest
{
    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;
}