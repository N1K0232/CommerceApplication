namespace CommerceApi.Shared.Models.Requests;

public class SaveSupplierRequest
{
    public string CompanyName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string City { get; set; } = null!;
}