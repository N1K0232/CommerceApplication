using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Constructor : BaseEntity
{
    public string Name { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string PostalCode { get; set; }

    public string WebSiteUrl { get; set; }

    public virtual IList<Product> Products { get; set; }
}