using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models.Common;

namespace CommerceApi.Shared.Models;

public class Coupon : BaseObject
{
    public User User { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public CouponStatus Status { get; set; }

    public bool IsValid { get; set; }
}