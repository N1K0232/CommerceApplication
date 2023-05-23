using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Cart : BaseObject
{
    public User User { get; set; } = null!;

    public IEnumerable<CartItem> CartItems { get; set; } = null!;
}