using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.Authentication.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class AddressMapperProfile : Profile
{
    public AddressMapperProfile()
    {
        CreateMap<Entities.Address, Address>();
        CreateMap<SaveAddressRequest, Entities.Address>();
    }
}