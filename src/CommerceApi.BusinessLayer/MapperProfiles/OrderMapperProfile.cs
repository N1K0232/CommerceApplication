using AutoMapper;
using CommerceApi.Shared.Models;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.MapperProfiles;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>()
            .ForMember(order => order.User, options => options.MapFrom(dbOrder => string.Join(dbOrder.User.FirstName, dbOrder.User.LastName)))
            .ForMember(order => order.Date, options => options.MapFrom(dbOrder => new DateTime(dbOrder.Date.Year, dbOrder.Date.Month, dbOrder.Date.Day)))
            .ForMember(order => order.Time, options => options.MapFrom(dbOrder => new TimeSpan(dbOrder.Time.Hour, dbOrder.Time.Minute, dbOrder.Time.Second)));
    }
}