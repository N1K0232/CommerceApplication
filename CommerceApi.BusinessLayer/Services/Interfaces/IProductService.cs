using CommerceApi.Shared.Common;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IProductService
{
    Task<Result> DeleteAsync(Guid productId);

    Task<Result<Product>> GetAsync(Guid productId);

    Task<ListResult<Product>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<Product>> SaveAsync(SaveProductRequest request);
}