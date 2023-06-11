using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CouponService : ICouponService
{
    private readonly IDataContext _dataContext;
    private readonly IUserClaimService _claimService;
    private readonly IMapper _mapper;

    public CouponService(IDataContext dataContext, IUserClaimService claimService, IMapper mapper)
    {
        _dataContext = dataContext;
        _claimService = claimService;
        _mapper = mapper;
    }

    public async Task<Result<Coupon>> CreateAsync(SaveCouponRequest coupon)
    {
        try
        {
            var dbCoupon = _mapper.Map<Entities.Coupon>(coupon);
            dbCoupon.UserId = _claimService.GetId();
            dbCoupon.Status = CouponStatus.New;
            dbCoupon.IsValid = true;

            _dataContext.Create(dbCoupon);
            await _dataContext.SaveAsync();

            var savedCoupon = _mapper.Map<Coupon>(dbCoupon);
            return savedCoupon;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid couponId)
    {
        if (couponId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.GetData<Entities.Coupon>(trackingChanges: true);
            var coupon = await query.FirstOrDefaultAsync(c => c.Id == couponId);
            if (coupon != null)
            {
                coupon.Status = CouponStatus.Deleted;
                coupon.IsValid = false;

                _dataContext.Delete(coupon);
                await _dataContext.SaveAsync();
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, "No coupon found");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}