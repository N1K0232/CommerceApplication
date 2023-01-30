using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Models;

public class Product : BaseModel
{
    public Category? Category { get; init; }

    public Supplier? Supplier { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? Specifications { get; init; }

    public string? Brand { get; init; }

    public string? Model { get; init; }

    public ProductCondition? Condition { get; init; }

    public ProductStatus? Status { get; init; }

    public int? Quantity { get; init; }

    public decimal? Price { get; init; }
}