using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CommerceApi.Swagger;

internal class ProblemDetailsResultExecutor : IActionResultExecutor<ObjectResult>
{
    public Task ExecuteAsync(ActionContext context, ObjectResult result)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        var value = result.Value;
        var statusCode = result.StatusCode;
        var httpContext = context.HttpContext;

        var executor = Results.Json(value, null, "application/problem+json", statusCode);
        return executor.ExecuteAsync(httpContext);
    }
}