using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
using CommerceApi.Shared.Common;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveCategoryRequest> categoryValidator;
    private readonly ILogger<CategoryService> logger;

    public CategoryService(IDataContext dataContext,
        IMapper mapper,
        IValidator<SaveCategoryRequest> categoryValidator,
        ILogger<CategoryService> logger)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.categoryValidator = categoryValidator;
        this.logger = logger;
    }


    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        logger.LogInformation("deleting category");

        if (categoryId == Guid.Empty)
        {
            logger.LogError("Invalid id", categoryId);
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetAsync<Entities.Category>(categoryId);
        if (category is not null)
        {
            dataContext.Delete(category);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                logger.LogInformation("category successfully deleted");
                return Result.Ok();
            }

            logger.LogError("cannot delete category");
            return Result.Fail(FailureReasons.DatabaseError, "cannot delete category");
        }

        logger.LogError("no category found");
        return Result.Fail(FailureReasons.ItemNotFound, "no category found");
    }

    public async Task<Result<Category>> GetAsync(Guid categoryId)
    {
        logger.LogInformation("get the single category");

        if (categoryId == Guid.Empty)
        {
            logger.LogError("invalid id", categoryId);
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var category = await dataContext.GetData<Entities.Category>()
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is not null)
        {
            return category;
        }

        logger.LogError("no category found");
        return Result.Fail(FailureReasons.ItemNotFound, "no category found");
    }

    public async Task<ListResult<Category>> GetListAsync()
    {
        logger.LogInformation("get list of categories");

        var categories = await dataContext.GetData<Entities.Category>()
            .OrderBy(c => c.Name)
            .ProjectTo<Category>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Category>();
        result.Content = categories;
        result.TotalCount = categories.Count;
        result.HasNextPage = false;

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

            logger.LogError("Invalid request", validationErrors);
            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var category = request.Id != null ? await dataContext.GetData<Entities.Category>(trackingChanges: true)
            .FirstOrDefaultAsync(c => c.Id == request.Id) : null;

        if (category is null)
        {
            logger.LogInformation("saving new category", request);

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
            logger.LogInformation("updating category");

            mapper.Map(request, category);
            dataContext.Edit(category);
        }

        var savedEntries = await dataContext.SaveAsync();
        if (savedEntries > 0)
        {
            logger.LogInformation("category successfully saved");

            var savedCategory = mapper.Map<Category>(category);
            return savedCategory;
        }

        logger.LogError("cannot save the category", request);
        return Result.Fail(FailureReasons.DatabaseError, "cannot save the category");
    }
}