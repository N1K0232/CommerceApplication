using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<Category>> CreateAsync(SaveCategoryRequest category);

    Task<Result> DeleteAsync(Guid categoryId);

    Task<Result<Category>> GetAsync(Guid categoryId);

    Task<IEnumerable<Category>> GetListAsync();

    Task<Result<Category>> UpdateAsync(Guid categoryId, SaveCategoryRequest category);
}