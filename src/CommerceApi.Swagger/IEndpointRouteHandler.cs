using Microsoft.AspNetCore.Routing;

namespace CommerceApi.Swagger;

public interface IEndpointRouteHandler
{
    void MapEndpoints(IEndpointRouteBuilder builder);
}