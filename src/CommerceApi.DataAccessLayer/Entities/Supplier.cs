using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Supplier : BaseEntity
{
    public string CompanyName { get; set; }

    public string ContactName { get; set; }

    public string City { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}