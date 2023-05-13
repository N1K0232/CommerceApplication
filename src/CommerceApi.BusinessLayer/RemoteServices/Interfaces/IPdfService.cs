using CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.RemoteServices.Interfaces;

public interface IPdfService
{
    Task UploadPdfInvoiceAsync(Invoice invoice);
}