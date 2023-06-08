using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Create(SaveSupplierRequest supplier)
    {
        var result = await _supplierService.CreateAsync(supplier);
        return CreateResponse(result, StatusCodes.Status201Created);
    }

    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid supplierId)
    {
        var result = await _supplierService.DeleteAsync(supplierId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet("{supplierId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Get(Guid supplierId)
    {
        var supplierResult = await _supplierService.GetAsync(supplierId);
        return CreateResponse(supplierResult, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> GetList()
    {
        var suppliers = await _supplierService.GetListAsync();
        return Ok(suppliers);
    }

    [HttpPut]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Update(Guid supplierId, SaveSupplierRequest supplier)
    {
        var result = await _supplierService.UpdateAsync(supplierId, supplier);
        return CreateResponse(result, StatusCodes.Status200OK);
    }
}