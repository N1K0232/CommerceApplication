namespace CommerceApi.Shared.Models.Requests;

public class SaveCouponRequest
{
    public string Code { get; set; } = null!;

    public double DiscountPercentage { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpirationDate { get; set; }
}