using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CategoryService : ICategoryService
{
    private readonly IDataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveCategoryRequest> _categoryValidator;

    public CategoryService(IDataContext dataContext, IMapper mapper, IValidator<SaveCategoryRequest> categoryValidator)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _categoryValidator = categoryValidator;
    }

    public async Task<Result<Category>> CreateAsync(SaveCategoryRequest category)
    {
        var validationResult = await _categoryValidator.ValidateAsync(category);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var dbCategory = _mapper.Map<Entities.Category>(category);
            _dataContext.Create(dbCategory);

            await _dataContext.SaveAsync();

            var savedCategory = _mapper.Map<Category>(dbCategory);
            return savedCategory;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var dbCategory = await _dataContext.GetAsync<Entities.Category>(categoryId);
            if (dbCategory is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"category not found with id {categoryId}");
            }

            _dataContext.Delete(dbCategory);
            await _dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Category>> GetAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var dbCategory = await _dataContext.GetAsync<Entities.Category>(categoryId);
        if (dbCategory is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"category not found with id {categoryId}");
        }

        var category = _mapper.Map<Category>(dbCategory);
        return category;
    }

    public async Task<IEnumerable<Category>> GetListAsync()
    {
        var query = _dataContext.GetData<Entities.Category>();
        var dbCategories = await query.OrderBy(c => c.Name).ToListAsync();

        var categories = _mapper.Map<IEnumerable<Category>>(dbCategories);
        return categories;
    }

    public async Task<Result<Category>> UpdateAsync(Guid categoryId, SaveCategoryRequest category)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var validationResult = await _categoryValidator.ValidateAsync(category);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var dbCategory = await _dataContext.GetAsync<Entities.Category>(categoryId);
            if (dbCategory is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"category not found with id {categoryId}");
            }

            _mapper.Map(category, dbCategory);
            _dataContext.Update(dbCategory);
            await _dataContext.SaveAsync();

            var savedCategory = _mapper.Map<Category>(dbCategory);
            return savedCategory;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}