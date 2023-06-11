using CommerceApi.Authentication.Entities;
using CommerceApi.DataAccessLayer.Entities.Common;
using CommerceApi.Shared.Enums;

namespace CommerceApi.DataAccessLayer.Entities;

public class Coupon : DeletableEntity
{
    public Guid UserId { get; set; }

    public string Code { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public CouponStatus Status { get; set; }

    public bool IsValid { get; set; }

    public virtual ApplicationUser User { get; set; }
}