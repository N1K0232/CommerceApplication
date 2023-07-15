using AutoMapper;
using CommerceApi.Shared.Models;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class ImageMapperProfile : Profile
{
    public ImageMapperProfile()
    {
        CreateMap<Entities.Image, Image>();
    }
}