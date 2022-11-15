using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<Entities.Product, Product>();
        CreateMap<SaveProductRequest, Entities.Product>();
    }
}