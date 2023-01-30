using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Requests;

public class SaveProductRequest : BaseRequestModel
{
    public Guid? CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Specifications { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public ProductCondition? Condition { get; set; }

    public ProductStatus? Status { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }
}