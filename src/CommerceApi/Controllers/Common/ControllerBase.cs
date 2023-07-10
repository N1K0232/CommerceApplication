using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OperationResults;
using OperationResults.AspNetCore;

namespace CommerceApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
    protected ControllerBase()
    {
    }

    protected IActionResult CreateResponse(Result result, int? successStatusCode = null)
    {
        return HttpContext.CreateResponse(result, successStatusCode);
    }

    protected IActionResult CreateResponse<T>(Result<T> result, int? successStatusCode = null)
    {
        return HttpContext.CreateResponse(result, successStatusCode);
    }

    protected IActionResult CreateResponse<T>(Result<T> result, string routeName, object routeValues = null, int? successStatusCode = null)
    {
        return HttpContext.CreateResponse(result, routeName, routeValues, successStatusCode);
    }
}