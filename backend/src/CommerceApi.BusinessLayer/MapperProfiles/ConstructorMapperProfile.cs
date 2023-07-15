using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class ConstructorMapperProfile : Profile
{
    public ConstructorMapperProfile()
    {
        CreateMap<Entities.Constructor, Constructor>();
        CreateMap<SaveConstructorRequest, Entities.Constructor>();
    }
}