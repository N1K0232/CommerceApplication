using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Product : DeletableEntity
{
    public Guid CategoryId { get; set; }

    public Guid SupplierId { get; set; }

    public string Name { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }

    public bool HasDiscount { get; set; }

    public virtual Category Category { get; set; }

    public virtual Supplier Supplier { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; }

    public virtual IList<OrderDetail> OrderDetails { get; set; }
}