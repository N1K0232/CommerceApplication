using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Models;

public class Order : BaseModel
{
    public User? User { get; init; }

    public OrderStatus? Status { get; init; }

    public IEnumerable<Product>? Products { get; init; }
}