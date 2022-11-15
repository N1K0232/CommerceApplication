using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
using CommerceApi.Shared.Common;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveCategoryRequest> categoryValidator;

    public CategoryService(IDataContext dataContext, IMapper mapper, IValidator<SaveCategoryRequest> categoryValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.categoryValidator = categoryValidator;
    }


    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetAsync<Entities.Category>(categoryId);
        if (category is not null)
        {
            dataContext.Delete(category);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "cannot delete category");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no category found");
    }

    public async Task<Result<Category>> GetAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetData<Entities.Category>()
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is not null)
        {
            return category;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no category found");
    }

    public async Task<ListResult<Category>> GetListAsync()
    {
        var categories = await dataContext.GetData<Entities.Category>()
            .OrderBy(c => c.Name)
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Category>(categories);
        return result;
    }

    public async Task<Result<Category>> SaveAsync(SaveCategoryRequest request)
    {
        var validationResult = await categoryValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var category = request.Id != null ? await dataContext.GetData<Entities.Category>(trackingChanges: true)
            .FirstOrDefaultAsync(c => c.Id == request.Id) : null;

        if (category is null)
        {
            category = mapper.Map<Entities.Category>(request);

            var categoryExists = await dataContext.ExistsAsync<Entities.Category>(c => c.Name == category.Name);
            if (categoryExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the category already exists");
            }

            dataContext.Create(category);
        }
        else
        {
            mapper.Map(request, category);
            dataContext.Edit(category);
        }

        var savedEntries = await dataContext.SaveAsync();
        if (savedEntries > 0)
        {
            var savedCategory = mapper.Map<Category>(category);
            return savedCategory;
        }

        return Result.Fail(FailureReasons.DatabaseError, "cannot save the category");
    }
}