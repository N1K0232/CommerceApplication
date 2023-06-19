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
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class ProductService : IProductService
{
    private readonly IDataContext _dataContext;
    private readonly IStorageProvider _storageProvider;
    private readonly IPathGenerator _pathGenerator;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveProductRequest> _productValidator;
    private readonly IValidator<SaveReviewRequest> _reviewValidator;
    private readonly IUserClaimService _claimService;

    public ProductService(IDataContext dataContext,
        IStorageProvider storageProvider,
        IPathGenerator pathGenerator,
        IMapper mapper,
        IValidator<SaveProductRequest> productValidator,
        IValidator<SaveReviewRequest> reviewValidator,
        IUserClaimService claimService)
    {
        _dataContext = dataContext;
        _storageProvider = storageProvider;
        _pathGenerator = pathGenerator;
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
            var query = _dataContext.GetData<Entities.Product>();
            var productExists = await query.AnyAsync(p => p.Name == product.Name);
            if (productExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the product already exists");
            }

            var dbProduct = _mapper.Map<Entities.Product>(product);
            dbProduct.HasDiscount = product.DiscountPercentage.GetValueOrDefault() > 0;
            dbProduct.HasShipping = product.ShippingCost.GetValueOrDefault() > 0;
            dbProduct.IdentificationCode = random.NextInt64(1, long.MaxValue).ToString();

            _dataContext.Create(dbProduct);
            await _dataContext.SaveAsync();

            var savedProduct = _mapper.Map<Product>(dbProduct);
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

        var dbProduct = await _dataContext.GetData<Entities.Product>().FirstOrDefaultAsync(p => p.Id == productId);
        if (dbProduct == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
        }

        var product = _mapper.Map<Product>(dbProduct);
        return product;
    }

    public async Task<ListResult<Product>> GetListAsync(string name, string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = _dataContext.GetData<Entities.Product>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var totalCount = await query.CountAsync();
        var dbProducts = await query.Include(p => p.Category)
            .Skip(pageIndex * itemsPerPage)
            .Take(itemsPerPage + 1)
            .ToListAsync();

        var products = _mapper.Map<IEnumerable<Product>>(dbProducts).Take(itemsPerPage);

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
            var product = await _dataContext.GetData<Entities.Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            _dataContext.Delete(product);
            await _dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> UploadImageAsync(Guid productId, string fileName, Stream fileStream)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.GetData<Entities.Product>();
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
            var query = _dataContext.GetData<Entities.Product>(ignoreQueryFilters: true, trackingChanges: true);
            var dbProduct = await query.FirstOrDefaultAsync(p => p.Id == productId);
            if (dbProduct is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No product found with id {productId}");
            }

            _mapper.Map(product, dbProduct);
            dbProduct.HasDiscount = product.DiscountPercentage.GetValueOrDefault() > 0;
            dbProduct.HasShipping = product.ShippingCost.GetValueOrDefault() > 0;

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

    private async Task UpdateAverageScoreAsync(Guid productId)
    {
        var query = _dataContext.GetData<Entities.Product>(trackingChanges: true);
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

    public async Task<Result<IEnumerable<Review>>> GetReviewsAsync(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var query = _dataContext.GetData<Entities.Product>();

        var productExists = await query.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "the product doesn't exists");
        }

        var dbReviews = await query.Include(p => p.Reviews)
            .Where(p => p.Id == productId)
            .Select(p => p.Reviews)
            .ToListAsync();

        var reviews = _mapper.Map<IEnumerable<Review>>(dbReviews);
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
}