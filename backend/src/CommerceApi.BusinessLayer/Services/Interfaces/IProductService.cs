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

    Task<ListResult<Product>> GetListAsync(string name, string category, string orderBy, int pageIndex, int itemsPerPage);

    Task<Result<Product>> UpdateAsync(Guid productId, SaveProductRequest product);

    Task<Result<IEnumerable<StreamFileContent>>> GetImagesAsync(Guid productId);

    Task<Result> UploadImageAsync(Guid productId, string fileName, Stream fileStream);

    Task<Result> AddReviewAsync(SaveReviewRequest review);

    Task<Result> DeleteReviewAsync(Guid reviewId);

    Task<Result<IEnumerable<Review>>> GetReviewsAsync(Guid productId);

    Task<Result> UpdateReviewAsync(Guid reviewId, SaveReviewRequest review);
}