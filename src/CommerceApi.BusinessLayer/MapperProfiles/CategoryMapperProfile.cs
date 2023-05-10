using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class CategoryMapperProfile : Profile
{
    public CategoryMapperProfile()
    {
        CreateMap<Entities.Category, Category>();
        CreateMap<SaveCategoryRequest, Entities.Category>();
    }
}