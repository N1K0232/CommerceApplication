namespace CommerceApi.Shared.Models.Requests;

public class SaveConstructorRequest
{
    public string Name { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? WebSiteUrl { get; set; }
}