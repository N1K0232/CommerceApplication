using CommerceApi.Shared.Common;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICategoryService
{
    Task<Result> DeleteAsync(Guid categoryId);

    Task<Result<Category>> GetAsync(Guid categoryId);

    Task<ListResult<Category>> GetListAsync();

    Task<Result<Category>> SaveAsync(SaveCategoryRequest request);
}