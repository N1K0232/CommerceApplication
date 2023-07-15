using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class SupplierMapperProfile : Profile
{
    public SupplierMapperProfile()
    {
        CreateMap<Entities.Supplier, Supplier>();
        CreateMap<SaveSupplierRequest, Entities.Supplier>();
    }
}