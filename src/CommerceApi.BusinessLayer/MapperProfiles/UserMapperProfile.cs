using AutoMapper;
using CommerceApi.Authentication.Entities;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<AuthenticationUser, User>();
        CreateMap<RegisterRequest, AuthenticationUser>();
    }
}