using CommerceApi.BusinessLayer.Models;

namespace CommerceApi.BusinessLayer.RemoteServices.Interfaces;
public interface ITenantService
{
    Tenant Get();
}