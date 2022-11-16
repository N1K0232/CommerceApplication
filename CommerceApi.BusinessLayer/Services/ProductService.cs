using System.Linq.Dynamic.Core;
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

public class ProductService : IProductService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveProductRequest> productValidator;

    public ProductService(IDataContext dataContext, IMapper mapper, IValidator<SaveProductRequest> productValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.productValidator = productValidator;
    }


    public async Task<Result> DeleteAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var product = await dataContext.GetAsync<Entities.Product>(productId);
        if (product is not null)
        {
            dataContext.Delete(product);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "cannot delete product");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no product found");
    }

    public async Task<Result<Product>> GetAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var product = await dataContext.GetData<Entities.Product>()
            .Include(p => p.Category)
            .ProjectTo<Product>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is not null)
        {
            return product;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no product found");
    }

    public async Task<ListResult<Product>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dataContext.GetData<Entities.Product>();

        var totalCount = await query.CountAsync();
        var products = await query.Include(p => p.Category)
            .OrderBy(orderBy)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ProjectTo<Product>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Product>(products.Take(itemsPerPage), totalCount, products.Count > itemsPerPage);
        return result;
    }

    public async Task<Result<Product>> SaveAsync(SaveProductRequest request)
    {
        var validationResult = await productValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();

            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var categoryExists = await dataContext.ExistsAsync<Entities.Category>(request.CategoryId);
        if (!categoryExists)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "invalid category");
        }

        var product = request.Id != null ? await dataContext.GetData<Entities.Product>(ignoreAutoIncludes: false, ignoreQueryFilters: true, trackingChanges: true)
            .FirstOrDefaultAsync(p => p.Id == request.Id) : null;

        if (product is null)
        {
            product = mapper.Map<Entities.Product>(request);

            var productExists = await dataContext.ExistsAsync<Entities.Product>(p => p.Name == product.Name);
            if (productExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the product already exists");
            }

            dataContext.Create(product);
        }
        else
        {
            mapper.Map(request, product);
            dataContext.Edit(product);
        }

        var savedEntries = await dataContext.SaveAsync();
        if (savedEntries > 0)
        {
            var savedProduct = mapper.Map<Product>(product);
            savedProduct.Category = mapper.Map<Category>(product.Category);
            return savedProduct;
        }

        return Result.Fail(FailureReasons.DatabaseError, "cannot save product");
    }
}