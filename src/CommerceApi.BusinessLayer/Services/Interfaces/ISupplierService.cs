using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ISupplierService
{
    Task<Result> DeleteAsync(Guid supplierId);

    Task<IEnumerable<Supplier>> GetAsync();

    Task<Result<Supplier>> GetAsync(Guid supplierId);

    Task<Result<Supplier>> SaveAsync(SaveSupplierRequest request);
}