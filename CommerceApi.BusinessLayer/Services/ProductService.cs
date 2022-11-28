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
using Microsoft.Extensions.Logging;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveProductRequest> productValidator;
    private readonly ILogger<ProductService> logger;

    public ProductService(IDataContext dataContext,
        IMapper mapper,
        IValidator<SaveProductRequest> productValidator,
        ILogger<ProductService> logger)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.productValidator = productValidator;
        this.logger = logger;
    }


    public async Task<Result> DeleteAsync(Guid productId)
    {
        logger.LogInformation("deleting product");

        if (productId == Guid.Empty)
        {
            logger.LogError("Invalid id", productId);
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var product = await dataContext.GetData<Entities.Product>(trackingChanges: true)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is not null)
        {
            dataContext.Delete(product);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                logger.LogInformation("product successfully deleted");
                return Result.Ok();
            }

            logger.LogError("cannot delete product");
            return Result.Fail(FailureReasons.DatabaseError, "cannot delete product");
        }

        logger.LogError("no product found");
        return Result.Fail(FailureReasons.ItemNotFound, "no product found");
    }

    public async Task<Result<Product>> GetAsync(Guid productId)
    {
        logger.LogInformation("get the single product");

        if (productId == Guid.Empty)
        {
            logger.LogError("Invalid id", productId);
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

        logger.LogError("no product found");
        return Result.Fail(FailureReasons.ItemNotFound, "no product found");
    }

    public async Task<ListResult<Product>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        logger.LogInformation("get the list of products");

        var query = dataContext.GetData<Entities.Product>();

        if (orderBy.HasValue())
        {
            query = query.OrderBy(orderBy);
        }

        var totalCount = await query.CountAsync();

        var products = await query.Include(p => p.Category)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ProjectTo<Product>(mapper.ConfigurationProvider)
            .ToListAsync();

        var result = new ListResult<Product>();
        result.Content = products.Take(itemsPerPage);
        result.TotalCount = totalCount;
        result.HasNextPage = products.Count > itemsPerPage;

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

            logger.LogError("Invalid request", validationErrors);
            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var categoryExists = await dataContext.ExistsAsync<Entities.Category>(request.CategoryId);
        if (!categoryExists)
        {
            logger.LogError("invalid category", request.CategoryId);
            return Result.Fail(FailureReasons.ItemNotFound, "invalid category");
        }

        var product = request.Id != null ? await dataContext.GetData<Entities.Product>(ignoreAutoIncludes: false, ignoreQueryFilters: true, trackingChanges: true)
            .FirstOrDefaultAsync(p => p.Id == request.Id) : null;

        if (product is null)
        {
            logger.LogInformation("saving new product");

            product = mapper.Map<Entities.Product>(request);

            var productExists = await dataContext.ExistsAsync<Entities.Product>(p => p.Name == product.Name);
            if (productExists)
            {
                logger.LogError("the product already exists");
                return Result.Fail(FailureReasons.Conflict, "the product already exists");
            }

            dataContext.Create(product);
        }
        else
        {
            logger.LogInformation("updating product");

            mapper.Map(request, product);
            dataContext.Edit(product);
        }

        var savedEntries = await dataContext.SaveAsync();
        if (savedEntries > 0)
        {
            logger.LogInformation("product successfully saved");

            var savedProduct = mapper.Map<Product>(product);
            savedProduct.Category = mapper.Map<Category>(product.Category);
            return savedProduct;
        }

        logger.LogError("cannot save product");
        return Result.Fail(FailureReasons.DatabaseError, "cannot save product");
    }
}