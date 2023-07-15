using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("AddItem")]
    public async Task<IActionResult> AddItem(SaveItemRequest item)
    {
        var result = await _cartService.AddItemAsync(item);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpDelete("ClearCart")]
    public async Task<IActionResult> ClearCart(Guid cartId)
    {
        var result = await _cartService.ClearCartAsync(cartId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create()
    {
        var result = await _cartService.CreateAsync();
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("Items")]
    public async Task<IActionResult> GetItems(Guid cartId)
    {
        var items = await _cartService.GetItemsAsync(cartId);
        return CreateResponse(items, StatusCodes.Status200OK);
    }

    [HttpGet("SubTotal")]
    public async Task<IActionResult> GetSubTotal(Guid cartId)
    {
        var result = await _cartService.GetSubTotalAsync(cartId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpDelete("RemoveItem")]
    public async Task<IActionResult> RemoveItem(Guid cartId, Guid itemId)
    {
        var result = await _cartService.RemoveItemAsync(cartId, itemId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}