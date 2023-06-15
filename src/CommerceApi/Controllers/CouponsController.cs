using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveCouponRequest coupon)
    {
        var result = await _couponService.CreateAsync(coupon);
        return CreateResponse(result, StatusCodes.Status201Created);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid couponId)
    {
        var result = await _couponService.DeleteAsync(couponId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("GetList")]
    public async Task<IActionResult> GetList(Guid userId)
    {
        var coupons = await _couponService.GetListAsync(userId);
        return Ok(coupons);
    }

    [HttpGet("{couponId}")]
    public async Task<IActionResult> Get(Guid couponId)
    {
        var result = await _couponService.GetAsync(couponId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid couponId, SaveCouponRequest coupon)
    {
        var result = await _couponService.UpdateAsync(couponId, coupon);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPut("updatestatus")]
    public async Task<IActionResult> UpdateStatus(Guid couponId, CouponStatus status)
    {
        var result = await _couponService.UpdateStatusAsync(couponId, status);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}