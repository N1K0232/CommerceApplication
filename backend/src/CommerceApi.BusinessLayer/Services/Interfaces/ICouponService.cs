using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICouponService
{
    Task<Result<Coupon>> CreateAsync(SaveCouponRequest coupon);

    Task<Result> DeleteAsync(Guid couponId);

    Task<IEnumerable<Coupon>> GetListAsync(Guid userId);

    Task<Result<Coupon>> GetAsync(Guid couponId);

    Task<Result<Coupon>> UpdateAsync(Guid couponId, SaveCouponRequest coupon);

    Task<Result<Coupon>> UpdateStatusAsync(Guid couponId, CouponStatus status);
}