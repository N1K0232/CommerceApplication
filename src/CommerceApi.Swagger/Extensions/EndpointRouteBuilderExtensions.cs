using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace CommerceApi.Swagger.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        var endpointRouteHandlerType = typeof(IEndpointRouteHandler);
        var endpointRouteHandlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                   t.GetConstructor(Type.EmptyTypes) != null &&
                   endpointRouteHandlerType.IsAssignableFrom(t));

        foreach (var endpointRouteHandler in endpointRouteHandlers)
        {
            var handler = (IEndpointRouteHandler)Activator.CreateInstance(endpointRouteHandler)!;
            handler.MapEndpoints(builder);
        }

        return builder;
    }
}