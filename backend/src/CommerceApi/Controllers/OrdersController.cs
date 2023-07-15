using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("AddDetails")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> AddDetails([FromBody] SaveOrderDetail order)
    {
        var result = await _orderService.AddDetailsAsync(order);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("Create")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> Create()
    {
        var result = await _orderService.CreateAsync();
        return CreateResponse(result, StatusCodes.Status202Accepted);
    }

    [HttpDelete("Cancel")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var result = await _orderService.DeleteAsync(orderId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("GetOrder/{orderId}")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var order = await _orderService.GetAsync(orderId);
        return CreateResponse(order, StatusCodes.Status200OK);
    }

    [HttpGet("GetList")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> GetList(string orderBy = "Date DESC", int pageIndex = 0, int itemsPerPage = 50)
    {
        var orders = await _orderService.GetListAsync(orderBy, pageIndex, itemsPerPage);
        return Ok(orders);
    }

    [HttpGet("GetTotal")]
    [RoleAuthorize(RoleNames.User)]
    public async Task<IActionResult> GetTotalPrice(Guid orderId)
    {
        var result = await _orderService.GetTotalPriceAsync(orderId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPut("UpdateStatus")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateStatusAsync(request);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}