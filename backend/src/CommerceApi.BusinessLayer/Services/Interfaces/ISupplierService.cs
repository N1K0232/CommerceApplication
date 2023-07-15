using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;
public interface ISupplierService
{
    Task<Result<Supplier>> CreateAsync(SaveSupplierRequest supplier);

    Task<Result> DeleteAsync(Guid supplierId);

    Task<Result<Supplier>> GetAsync(Guid supplierId);

    Task<IEnumerable<Supplier>> GetListAsync();

    Task<Result<Supplier>> UpdateAsync(Guid supplierId, SaveSupplierRequest supplier);
}