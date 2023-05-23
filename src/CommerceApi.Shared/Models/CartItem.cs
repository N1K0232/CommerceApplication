using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class CartItem : BaseObject
{
    public Product Product { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}