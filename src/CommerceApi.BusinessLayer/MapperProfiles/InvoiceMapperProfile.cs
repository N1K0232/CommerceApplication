using AutoMapper;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class InvoiceMapperProfile : Profile
{
    public InvoiceMapperProfile()
    {
        CreateMap<Entities.Invoice, Invoice>();
        CreateMap<SaveInvoiceRequest, Entities.Invoice>();
    }
}