using CommerceApi.Shared.Common;

namespace CommerceApi.Shared.Models;

public class Product : BaseModel
{
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }

    public bool HasDiscount { get; set; }
}