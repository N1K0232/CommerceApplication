using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class ProductsController : ControllerBase
{
    private readonly IProductService productService;

    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid productId)
    {
        var result = await productService.DeleteAsync(productId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(int pageIndex = 0, int itemsPerPage = 10, string orderBy = "Name")
    {
        var products = await productService.GetListAsync(pageIndex, itemsPerPage, orderBy);
        if (!products.Content.Any())
        {
            return NotFound("no product found");
        }

        return Ok(products);
    }

    [HttpGet("{productId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(Guid productId)
    {
        var productResult = await productService.GetAsync(productId);
        return CreateResponse(productResult, StatusCodes.Status200OK);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Save([FromBody] SaveProductRequest request)
    {
        var result = await productService.SaveAsync(request);
        return CreateResponse(result, StatusCodes.Status201Created);
    }
}