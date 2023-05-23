using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class CartItem : DeletableEntity
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; }

    public virtual Product Product { get; set; }
}