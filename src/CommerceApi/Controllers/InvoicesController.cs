using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        this.invoiceService = invoiceService;
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Create([FromBody] SaveInvoiceRequest invoice)
    {
        var savedInvoice = await invoiceService.CreateAsync(invoice);
        return CreateResponse(savedInvoice, StatusCodes.Status201Created);
    }

    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid invoiceId)
    {
        var deleteResult = await invoiceService.DeleteAsync(invoiceId);
        return CreateResponse(deleteResult, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> GetList()
    {
        var invoices = await invoiceService.GetListAsync();
        return Ok(invoices);
    }

    [HttpGet("{invoiceId:guid}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Get(Guid invoiceId)
    {
        var invoice = await invoiceService.GetAsync(invoiceId);
        return CreateResponse(invoice, StatusCodes.Status200OK);
    }

    [HttpPut]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Update(Guid invoiceId, SaveInvoiceRequest invoice)
    {
        var savedInvoice = await invoiceService.UpdateAsync(invoiceId, invoice);
        return CreateResponse(savedInvoice, StatusCodes.Status200OK);
    }
}