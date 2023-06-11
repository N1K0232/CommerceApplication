using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class CouponMapperProfile : Profile
{
    public CouponMapperProfile()
    {
        CreateMap<Entities.Coupon, Coupon>();
        CreateMap<SaveCouponRequest, Entities.Coupon>();
    }
}