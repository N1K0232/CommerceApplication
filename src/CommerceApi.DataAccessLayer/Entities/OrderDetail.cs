namespace CommerceApi.DataAccessLayer.Entities;

public class OrderDetail
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int OrderedQuantity { get; set; }

    public decimal Price { get; set; }

    public virtual Order Order { get; set; }

    public virtual Product Product { get; set; }
}