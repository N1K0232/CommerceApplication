using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class CartsController : ControllerBase
{
    private readonly ICartService cartService;

    public CartsController(ICartService cartService)
    {
        this.cartService = cartService;
    }

    [HttpPost("AddItem")]
    public async Task<IActionResult> AddItem(SaveItemRequest item)
    {
        var result = await cartService.AddItemAsync(item);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpDelete("ClearCart")]
    public async Task<IActionResult> ClearCart(Guid cartId)
    {
        var result = await cartService.ClearCartAsync(cartId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create()
    {
        var result = await cartService.CreateAsync();
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("Items")]
    public async Task<IActionResult> GetItems(Guid cartId)
    {
        var items = await cartService.GetItemsAsync(cartId);
        return CreateResponse(items, StatusCodes.Status200OK);
    }

    [HttpGet("SubTotal")]
    public async Task<IActionResult> GetSubTotal(Guid cartId)
    {
        var result = await cartService.GetSubTotalAsync(cartId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpDelete("RemoveItem")]
    public async Task<IActionResult> RemoveItem(Guid cartId, Guid itemId)
    {
        var result = await cartService.RemoveItemAsync(cartId, itemId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}