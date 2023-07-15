using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }

    public string Description { get; set; }

    public virtual IList<Product> Products { get; set; }
}