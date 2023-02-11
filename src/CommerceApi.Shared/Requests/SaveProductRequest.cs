using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Requests;

public class SaveProductRequest
{
    public Guid? CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Specifications { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public ProductCondition? Condition { get; set; }

    public ProductStatus? Status { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }
}