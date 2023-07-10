using System.Linq.Dynamic.Core;
using AutoMapper;
using CommerceApi.BusinessLayer.Extensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Security.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using CommerceApi.StorageProviders.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IDataContext _dataContext;
    private readonly IStorageProvider _storageProvider;
    private readonly IPathGenerator _pathGenerator;
    private readonly IDataProtector _dataProtector;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveProductRequest> _productValidator;
    private readonly IValidator<SaveReviewRequest> _reviewValidator;
    private readonly IUserClaimService _claimService;

    public ProductService(IDataContext dataContext,
        IStorageProvider storageProvider,
        IPathGenerator pathGenerator,
        IDataProtector dataProtector,
        IMapper mapper,
        IValidator<SaveProductRequest> productValidator,
        IValidator<SaveReviewRequest> reviewValidator,
        IUserClaimService claimService)
    {
        _dataContext = dataContext;
        _storageProvider = storageProvider;
        _pathGenerator = pathGenerator;
        _dataProtector = dataProtector;
        _mapper = mapper;
        _productValidator = productValidator;
        _reviewValidator = reviewValidator;
        _claimService = claimService;
    }

    public async Task<Result<Product>> CreateAsync(SaveProductRequest product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
        }

        try
        {
            var random = new Random();
            var query = _dataContext.Get<Entities.Product>()
                .Include(p => p.Category)
                .Include(p => p.Constructor)
                .Include(p => p.Supplier)
                .AsQueryable();


            var productExists = await query.AnyAsync(p => p.Name == product.Name);
            if (productExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the product already exists");
            }

            var dbProduct = _mapper.Map<Entities.Product>(product);
            dbProduct.IdentificationCode = random.NextInt64(1, long.MaxValue).ToString();
            dbProduct.IsPublished = true;

            _dataContext.Create(dbProduct);
            await _dataContext.SaveAsync();

            var savedDbProduct = await query.FirstAsync(p => p.Id == dbProduct.Id);
            var savedProduct = _mapper.Map<Product>(savedDbProduct);
            return savedProduct;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.Get<Entities.Product>().Where(p => p.Id == productId);
            var productExists = await query.AnyAsync();
            if (!productExists)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            var product = await query.FirstAsync();
            _dataContext.Delete(product);

            await _dataContext.SaveAsync();
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Product>> GetAsync(Guid productId)
    {
        var query = _dataContext.Get<Entities.Product>()
            .Include(p => p.Category)
            .Include(p => p.Constructor)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (productId != Guid.Empty)
        {
            query = query.Where(p => p.Id == productId);
        }
        else
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var productExists = await query.AnyAsync();
        if (!productExists)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
        }

        var dbProduct = await query.FirstAsync();

        var product = _mapper.Map<Product>(dbProduct);
        return product;
    }

    public async Task<ListResult<Product>> GetListAsync(string name, string category, string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = _dataContext.Get<Entities.Product>()
            .Include(p => p.Category)
            .Include(p => p.Constructor)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var dbProducts = await query.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1).ToListAsync();
        var products = _mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);

        var totalCount = await query.LongCountAsync();
        var totalPages = totalCount / itemsPerPage;
        var hasNextPage = dbProducts.Count > itemsPerPage;

        var result = new ListResult<Product>(products, totalCount, totalPages, hasNextPage);
        return result;
    }

    public async Task<Result> UploadImageAsync(Guid productId, string fileName, Stream fileStream)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.Get<Entities.Product>();
            var productExists = await query.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            var extension = Path.GetExtension(fileName);
            var filePath = _pathGenerator.Generate(@"\products", productId.ToString(), extension);

            await _storageProvider.SaveAsync(filePath, fileStream);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Product>> UpdateAsync(Guid productId, SaveProductRequest product)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
        }

        try
        {
            var query = _dataContext.Get<Entities.Product>(ignoreQueryFilters: true, trackingChanges: true).Where(p => p.Id == productId);
            var productExists = await query.AnyAsync();
            if (!productExists)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            var dbProduct = await query.FirstAsync();
            _mapper.Map(product, dbProduct);

            _dataContext.Update(dbProduct);
            await _dataContext.SaveAsync();

            var savedProduct = _mapper.Map<Product>(dbProduct);
            return savedProduct;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> AddReviewAsync(SaveReviewRequest review)
    {
        var validationResult = await _reviewValidator.ValidateAsync(review);
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
            var dbReview = _mapper.Map<Entities.Review>(review);
            dbReview.UserId = _claimService.GetId();

            _dataContext.Create(dbReview);
            await _dataContext.SaveAsync();
            await UpdateAverageScoreAsync(review.ProductId);

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<IEnumerable<Review>>> GetReviewsAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var query = _dataContext.Get<Entities.Product>().Where(p => p.Id == productId);

        var productExists = await query.AnyAsync();
        if (!productExists)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "the product doesn't exists");
        }

        var dbProduct = await query.Include(p => p.Reviews).FirstAsync();
        var reviews = _mapper.Map<IEnumerable<Review>>(dbProduct.Reviews);
        return Result<IEnumerable<Review>>.Ok(reviews);
    }

    public async Task<Result> DeleteReviewAsync(Guid reviewId)
    {
        if (reviewId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        try
        {
            var review = await _dataContext.GetAsync<Entities.Review>(reviewId);
            if (review != null)
            {
                _dataContext.Delete(review);
                await _dataContext.SaveAsync();

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, "No review found");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> UpdateReviewAsync(Guid reviewId, SaveReviewRequest review)
    {
        try
        {
            if (reviewId == Guid.Empty)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid id");
            }

            var validationResult = await _reviewValidator.ValidateAsync(review);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbReview = await _dataContext.GetAsync<Entities.Review>(reviewId);
            if (dbReview != null)
            {
                _mapper.Map(review, dbReview);
                _dataContext.Update(dbReview);

                await _dataContext.SaveAsync();
                await UpdateAverageScoreAsync(review.ProductId);

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, "No review found");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    private async Task UpdateAverageScoreAsync(Guid productId)
    {
        var query = _dataContext.Get<Entities.Product>(trackingChanges: true);
        var product = await query.Include(p => p.Reviews).FirstAsync(p => p.Id == productId);

        var score = 0;
        foreach (var review in product.Reviews)
        {
            score += review.Score;
        }

        product.AverageScore = score / product.Reviews.Count;
        _dataContext.Update(product);
        await _dataContext.SaveAsync();
    }
}