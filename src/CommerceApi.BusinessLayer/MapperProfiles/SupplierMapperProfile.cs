using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

internal class SupplierMapperProfile : Profile
{
    public SupplierMapperProfile()
    {
        CreateMap<Entities.Supplier, Supplier>();
        CreateMap<SaveSupplierRequest, Entities.Supplier>();
    }
}