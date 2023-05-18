using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class OrdersController : ControllerBase
{
    private readonly IOrderService orderService;

    public OrdersController(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpPost("AddDetails")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> AddDetails([FromBody] SaveOrderDetail order)
    {
        var result = await orderService.AddDetailsAsync(order);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Create")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> Create()
    {
        var result = await orderService.CreateAsync();
        return CreateResponse(result, StatusCodes.Status202Accepted);
    }

    [HttpDelete("Cancel")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var result = await orderService.DeleteAsync(orderId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("GetTotal")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> GetTotalPrice(Guid orderId)
    {
        var result = await orderService.GetTotalPriceAsync(orderId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}