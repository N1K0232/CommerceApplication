using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>();
        CreateMap<SaveOrderRequest, Entities.Order>();
    }
}