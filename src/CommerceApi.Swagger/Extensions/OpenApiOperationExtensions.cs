using Microsoft.OpenApi.Models;

namespace CommerceApi.Swagger.Extensions;

public static class OpenApiOperationExtensions
{
    public static void AddResponse(this OpenApiOperation operation, string key, string description)
    {
        var response = CreateResponse(description);
        operation.Responses.Add(key, response);
    }

    private static OpenApiResponse CreateResponse(string description)
    {
        var response = new OpenApiResponse { Description = description };
        return response;
    }
}