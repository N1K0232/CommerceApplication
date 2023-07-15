using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.DataAccessLayer.Entities;

public class Order : DeletableEntity
{
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; }

    public long IdentityNumber { get; set; }

    public string IdentificationNumber { get; set; }

    public string IdentificationCode { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public virtual IList<OrderDetail> OrderDetails { get; set; }
}