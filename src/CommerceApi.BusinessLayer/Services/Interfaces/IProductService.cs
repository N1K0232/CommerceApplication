using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IProductService
{
    Task<Result<Product>> CreateAsync(SaveProductRequest product);

    Task<Result> DeleteAsync(Guid productId);

    Task<Result<Product>> GetAsync(Guid productId);

    Task<ListResult<Product>> GetListAsync(string orderBy, int pageIndex, int itemsPerPage);

    Task<Result<Product>> UpdateAsync(Guid productId, SaveProductRequest product);

    Task<Result<Product>> UploadImageAsync(Guid productId, string fileName, Stream fileStream);
}