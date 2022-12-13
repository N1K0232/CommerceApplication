using CommerceApi.Authentication.Entities;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.DataAccessLayer.Entities;

public class Order : DeletableEntity
{
    public Guid UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public TimeSpan OrderTime { get; set; }

    public OrderStatus Status { get; set; }

    public virtual AuthenticationUser User { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}