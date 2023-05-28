namespace CommerceApi.Authentication.Entities;

public class Address
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public virtual ApplicationUser User { get; set; }
}