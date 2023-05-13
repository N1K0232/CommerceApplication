using CommerceApi.BusinessLayer.Services.Interfaces;

namespace CommerceApi.Controllers;

public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        this.invoiceService = invoiceService;
    }
}