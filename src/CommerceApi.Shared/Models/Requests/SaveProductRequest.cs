namespace CommerceApi.Shared.Models.Requests;

public class SaveProductRequest
{
    public Guid CategoryId { get; set; }

    public Guid SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }
}