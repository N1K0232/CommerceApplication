using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.Shared.Models;

public class Order : BaseModel
{
    public User User { get; set; }

    public OrderStatus Status { get; set; }

    public IEnumerable<Product> Products { get; set; }
}