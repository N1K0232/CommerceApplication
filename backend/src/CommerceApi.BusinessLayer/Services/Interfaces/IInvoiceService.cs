using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IInvoiceService
{
    Task<Result<Invoice>> CreateAsync(SaveInvoiceRequest invoice);

    Task<Result> DeleteAsync(Guid invoiceId);

    Task<Result<Invoice>> GetAsync(Guid invoiceId);

    Task<IEnumerable<Invoice>> GetListAsync();

    Task<Result<Invoice>> UpdateAsync(Guid invoiceId, SaveInvoiceRequest invoice);
}