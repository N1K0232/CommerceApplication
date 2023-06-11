using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("AddProduct")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] SaveProductRequest product)
    {
        var result = await _productService.CreateAsync(product);
        return CreateResponse(result, StatusCodes.Status201Created);
    }

    [HttpDelete("Delete")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid productId)
    {
        var result = await _productService.DeleteAsync(productId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("Get/{productId:guid}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(Guid productId)
    {
        var result = await _productService.GetAsync(productId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("GetList")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListResult<Product>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList(string orderBy = null, int pageIndex = 0, int itemsPerPage = 50)
    {
        var products = await _productService.GetListAsync(orderBy, pageIndex, itemsPerPage);
        return Ok(products);
    }

    [HttpPut("UpdateProduct")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid productId, SaveProductRequest product)
    {
        var result = await _productService.UpdateAsync(productId, product);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("UploadImage")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid productId, IFormFile file)
    {
        var result = await _productService.UploadImageAsync(productId, file.FileName, file.OpenReadStream());
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("reviews/AddReview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReview(SaveReviewRequest review)
    {
        var result = await _productService.AddReviewAsync(review);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("reviews/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviews(Guid productId)
    {
        var result = await _productService.GetReviewsAsync(productId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpDelete("reviews/{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var result = await _productService.DeleteReviewAsync(reviewId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPut("reviews/{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(Guid reviewId, SaveReviewRequest review)
    {
        var result = await _productService.UpdateReviewAsync(reviewId, review);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}