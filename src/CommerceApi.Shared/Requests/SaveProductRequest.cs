using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Requests;

public class SaveProductRequest : BaseRequestModel
{
    public Guid CategoryId { get; set; }

    public Guid SupplierId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Specifications { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public ProductCondition Condition { get; set; }

    public ProductStatus Status { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}