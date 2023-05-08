namespace CommerceApi.Shared.Requests;

public class SaveProductRequest
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }
}