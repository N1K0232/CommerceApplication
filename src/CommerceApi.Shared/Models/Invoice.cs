using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Invoice : BaseObject
{
    public string Product { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}