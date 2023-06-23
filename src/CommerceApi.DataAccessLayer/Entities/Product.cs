using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Product : DeletableEntity
{
    public Guid CategoryId { get; set; }

    public Guid ConstructorId { get; set; }

    public Guid SupplierId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string IdentificationCode { get; set; }

    public string Key { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public double? DiscountPercentage { get; set; }

    public bool HasDiscount { get; set; }

    public decimal? ShippingCost { get; set; }

    public bool HasShipping { get; set; }

    public double? AverageScore { get; set; }

    public bool IsPublished { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Category Category { get; set; }

    public virtual Constructor Constructor { get; set; }

    public virtual Supplier Supplier { get; set; }

    public virtual IList<Review> Reviews { get; set; }

    public virtual IList<Invoice> Invoices { get; set; }

    public virtual IList<Question> Questions { get; set; }

    public virtual IList<CartItem> CartItems { get; set; }

    public virtual IList<OrderDetail> OrderDetails { get; set; }
}