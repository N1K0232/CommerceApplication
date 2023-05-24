using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class CartMapperProfile : Profile
{
    public CartMapperProfile()
    {
        CreateMap<Entities.Cart, Cart>();
        CreateMap<Entities.CartItem, CartItem>();
        CreateMap<SaveItemRequest, Entities.CartItem>();
    }
}