using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<Entities.Product, Product>();
        CreateMap<SaveProductRequest, Entities.Product>()
            .ForMember(p => p.HasDiscount, destination => destination.MapFrom(source => source.DiscountPercentage.GetValueOrDefault() > 0))
            .ForMember(p => p.HasShipping, destination => destination.MapFrom(source => source.ShippingCost.GetValueOrDefault() > 0))
            .ForMember(p => p.IsAvailable, destination => destination.MapFrom(source => source.Quantity > 0));
    }
}