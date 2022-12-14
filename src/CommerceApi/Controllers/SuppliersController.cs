using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class SuppliersController : ControllerBase
{
    private readonly ISupplierService supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        this.supplierService = supplierService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid supplierId)
    {
        var result = await supplierService.DeleteAsync(supplierId);
        return CreateResponse(result);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Get()
    {
        var suppliers = await supplierService.GetAsync();
        if (!suppliers.Any())
        {
            return NotFound("no supplier found");
        }

        return Ok(suppliers);
    }

    [HttpGet("{supplierId}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Get(Guid supplierId)
    {
        var result = await supplierService.GetAsync(supplierId);
        return CreateResponse(result);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Save([FromBody] SaveSupplierRequest request)
    {
        var result = await supplierService.SaveAsync(request);
        return CreateResponse(result);
    }
}