using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICouponService
{
    Task<Result<Coupon>> CreateAsync(SaveCouponRequest coupon);

    Task<Result> DeleteAsync(Guid couponId);
}