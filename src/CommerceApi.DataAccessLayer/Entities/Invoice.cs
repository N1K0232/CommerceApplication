using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Invoice : BaseEntity
{
    public Guid ProductId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal Price { get; set; }

    public virtual Product Product { get; set; }
}