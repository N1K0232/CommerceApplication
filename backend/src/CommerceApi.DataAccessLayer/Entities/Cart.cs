using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Cart : DeletableEntity
{
    public Guid UserId { get; set; }

    public virtual IList<CartItem> CartItems { get; set; }
}