using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Product : BaseObject
{
    public Category Category { get; set; } = null!;

    public Constructor Constructor { get; set; } = null!;

    public Supplier Supplier { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }

    public bool HasDiscount { get; set; }

    public decimal? ShippingCost { get; set; }

    public bool HasShipping { get; set; }

    public double? AverageScore { get; set; }

    public bool IsPublished { get; set; }

    public bool IsAvailable { get; set; }
}