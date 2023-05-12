using System.Linq.Dynamic.Core;
using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.StorageProviders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IDataContext dataContext;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;
    private readonly IValidator<SaveProductRequest> productValidator;

    public ProductService(IDataContext dataContext,
        IStorageProvider storageProvider,
        IMapper mapper,
        IValidator<SaveProductRequest> productValidator)
    {
        this.dataContext = dataContext;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
        this.productValidator = productValidator;
    }

    public async Task<Result<Product>> CreateAsync(SaveProductRequest product)
    {
        var validationResult = await productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
        }

        try
        {
            var query = dataContext.GetData<Entities.Product>();
            var productExists = await query.AnyAsync(p => p.Name == product.Name);
            if (productExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the product already exists");
            }

            var dbProduct = mapper.Map<Entities.Product>(product);
            dbProduct.HasDiscount = dbProduct.DiscountPercentage.GetValueOrDefault() > 0;

            dataContext.Create(dbProduct);
            await dataContext.SaveAsync();

            var savedProduct = mapper.Map<Product>(dbProduct);
            return savedProduct;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Product>> GetAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var dbProduct = await dataContext.GetData<Entities.Product>().FirstOrDefaultAsync(p => p.Id == productId);
        if (dbProduct is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
        }

        var product = mapper.Map<Product>(dbProduct);
        return product;
    }

    public async Task<ListResult<Product>> GetListAsync(string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = dataContext.GetData<Entities.Product>();
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var totalCount = await query.CountAsync();
        var dbProducts = await query.Skip(pageIndex * itemsPerPage)
            .Take(itemsPerPage + 1)
            .ToListAsync();

        var products = mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);

        var result = new ListResult<Product>(products, totalCount, products.Count() > itemsPerPage);
        return result;
    }

    public async Task<Result> DeleteAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var product = await dataContext.GetData<Entities.Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            dataContext.Delete(product);
            await dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Product>> UploadImageAsync(Guid productId, string fileName, Stream fileStream)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = dataContext.GetData<Entities.Product>();
            var productExists = await query.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            var filePath = $@"\products\attachments\{productId}_{fileName}";
            await storageProvider.SaveAsync(filePath, fileStream);

            var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == productId);

            var savedProduct = mapper.Map<Product>(dbProduct);
            return savedProduct;
        }
        catch (Exception ex)
        {
            return Result.Fail(FailureReasons.GenericError, ex);
        }
    }

    public async Task<Result<Product>> UpdateAsync(Guid productId, SaveProductRequest product)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var validationResult = await productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
        }

        try
        {
            var query = dataContext.GetData<Entities.Product>(ignoreQueryFilters: true, trackingChanges: true);
            var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == productId);
            if (dbProduct is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            mapper.Map(product, dbProduct);
            dbProduct.HasDiscount = dbProduct.DiscountPercentage.GetValueOrDefault() > 0;

            dataContext.Edit(dbProduct);
            await dataContext.SaveAsync();

            var savedProduct = mapper.Map<Product>(dbProduct);
            return savedProduct;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}