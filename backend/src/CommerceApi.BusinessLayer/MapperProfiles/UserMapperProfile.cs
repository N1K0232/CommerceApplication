using AutoMapper;
using CommerceApi.Authentication.Entities;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<ApplicationUser, User>();
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(user => user.EmailConfirmed, options =>
            {
                options.MapFrom(source => source.Email == source.ConfirmEmail);
            })
            .ForMember(user => user.PhoneNumberConfirmed, options =>
            {
                options.MapFrom(source => source.PhoneNumber == source.ConfirmPhoneNumber);
            })
            .ForMember(user => user.UserName, options =>
            {
                options.MapFrom(source => source.UserName ?? source.Email);
            });
    }
}