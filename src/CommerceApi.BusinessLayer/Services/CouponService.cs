using AutoMapper;
using CommerceApi.BusinessLayer.RemoteServices;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CouponService : ICouponService
{
    private readonly IDataContext _dataContext;
    private readonly IDataProtector _dataProtector;
    private readonly IUserClaimService _claimService;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveCouponRequest> _couponValidator;

    public CouponService(IDataContext dataContext,
        IDataProtector dataProtector,
        IUserClaimService claimService,
        IMapper mapper,
        IValidator<SaveCouponRequest> couponValidator)
    {
        _dataContext = dataContext;
        _dataProtector = dataProtector;
        _claimService = claimService;
        _mapper = mapper;
        _couponValidator = couponValidator;
    }

    public async Task<Result<Coupon>> CreateAsync(SaveCouponRequest coupon)
    {
        var validationResult = await _couponValidator.ValidateAsync(coupon);
        if (!validationResult.IsValid)
        {
            var validationErrors = ValidationErrorService.GetErrors(validationResult);
            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var dbCoupon = new Entities.Coupon
            {
                UserId = _claimService.GetId(),
                Code = _dataProtector.Protect(coupon.Code),
                DiscountPercentage = coupon.DiscountPercentage,
                Status = CouponStatus.New,
                StartDate = coupon.StartDate,
                ExpirationDate = coupon.ExpirationDate,
            };

            _dataContext.Create(dbCoupon);
            await _dataContext.SaveAsync();

            var savedCoupon = _mapper.Map<Coupon>(dbCoupon);
            savedCoupon.Code = _dataProtector.Unprotect(dbCoupon.Code);

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

    public async Task<IEnumerable<Coupon>> GetListAsync(Guid userId)
    {
        var query = _dataContext.GetData<Entities.Coupon>();

        if (userId != Guid.Empty)
        {
            query = query.Where(c => c.UserId == userId);
        }

        var dbCoupons = await query.Include(c => c.User).ToListAsync();
        var coupons = _mapper.Map<IEnumerable<Coupon>>(dbCoupons);

        foreach (var coupon in coupons)
        {
            coupon.Code = _dataProtector.Unprotect(coupon.Code);
        }

        return coupons;
    }

    public async Task<Result<Coupon>> GetAsync(Guid couponId)
    {
        if (couponId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var query = _dataContext.GetData<Entities.Coupon>();
        var dbCoupon = await query.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == couponId);
        if (dbCoupon == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No coupon found with id {couponId}");
        }

        var coupon = _mapper.Map<Coupon>(dbCoupon);
        coupon.Code = _dataProtector.Unprotect(dbCoupon.Code);

        return coupon;
    }

    public async Task<Result<Coupon>> UpdateAsync(Guid couponId, SaveCouponRequest coupon)
    {
        if (couponId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var validationResult = await _couponValidator.ValidateAsync(coupon);
        if (!validationResult.IsValid)
        {
            var validationErrors = ValidationErrorService.GetErrors(validationResult);
            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var query = _dataContext.GetData<Entities.Coupon>(trackingChanges: true);
            var dbCoupon = await query.FirstOrDefaultAsync(c => c.Id == couponId);
            if (dbCoupon == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No coupon found with id {couponId}");
            }

            _mapper.Map(coupon, dbCoupon);
            _dataContext.Update(dbCoupon);
            await _dataContext.SaveAsync();

            var savedCoupon = _mapper.Map<Coupon>(dbCoupon);
            return savedCoupon;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Coupon>> UpdateStatusAsync(Guid couponId, CouponStatus status)
    {
        if (couponId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.GetData<Entities.Coupon>(trackingChanges: true);
            var dbCoupon = await query.FirstOrDefaultAsync(c => c.Id == couponId);
            if (dbCoupon == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No coupon found with id {couponId}");
            }

            dbCoupon.Status = status;
            _dataContext.Update(dbCoupon);
            await _dataContext.SaveAsync();

            var savedCoupon = _mapper.Map<Coupon>(dbCoupon);
            savedCoupon.Code = _dataProtector.Unprotect(dbCoupon.Code);

            return savedCoupon;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}