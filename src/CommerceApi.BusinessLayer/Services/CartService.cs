using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class CartService : ICartService
{
    private readonly IDataContext dataContext;
    private readonly IAuthenticationService authenticationService;
    private readonly IMapper mapper;
    private readonly IUserClaimService claimService;
    private readonly IValidator<SaveItemRequest> itemValidator;

    public CartService(IDataContext dataContext,
        IAuthenticationService authenticationService,
        IMapper mapper,
        IUserClaimService claimService,
        IValidator<SaveItemRequest> itemValidator)
    {
        this.dataContext = dataContext;
        this.authenticationService = authenticationService;
        this.mapper = mapper;
        this.claimService = claimService;
        this.itemValidator = itemValidator;
    }

    public async Task<Result> AddItemAsync(SaveItemRequest item)
    {
        try
        {
            var validationResult = await itemValidator.ValidateAsync(item);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
            }

            var dbItem = mapper.Map<Entities.CartItem>(item);
            dataContext.Create(dbItem);

            await dataContext.SaveAsync();
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> ClearCartAsync(Guid cartId)
    {
        if (cartId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = dataContext.GetData<Entities.Cart>();

            var dbCart = await query.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == cartId);
            if (dbCart is not null)
            {
                dataContext.Delete(dbCart.CartItems);
                await dataContext.SaveAsync();

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, $"No cart found with id {cartId}");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Cart>> CreateAsync()
    {
        try
        {
            var userId = claimService.GetId();
            var cartExists = await dataContext.GetData<Entities.Cart>().AnyAsync(c => c.UserId == userId);
            if (cartExists)
            {
                return Result.Fail(FailureReasons.Conflict, "cart already exists");
            }

            var dbCart = new Entities.Cart
            {
                UserId = userId
            };

            dataContext.Create(dbCart);
            await dataContext.SaveAsync();

            var savedCart = mapper.Map<Cart>(dbCart);
            savedCart.User = await authenticationService.GetAsync();

            return savedCart;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> RemoveItemAsync(Guid cartId, Guid itemId)
    {
        if (cartId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        if (itemId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = dataContext.GetData<Entities.CartItem>(trackingChanges: true);
            var cartItem = await query.FirstOrDefaultAsync(c => c.Id == itemId && c.CartId == cartId);
            if (cartItem is not null)
            {
                dataContext.Delete(cartItem);
                await dataContext.SaveAsync();

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, $"No item found");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<IEnumerable<CartItem>>> GetItemsAsync(Guid cartId)
    {
        if (cartId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var userId = claimService.GetId();
        var query = dataContext.GetData<Entities.Cart>().Where(c => c.Id == cartId && c.UserId == userId);

        var cart = await query.Include(c => c.CartItems).FirstOrDefaultAsync();
        var cartItems = mapper.Map<IEnumerable<CartItem>>(cart.CartItems);

        var result = Result<IEnumerable<CartItem>>.Ok(cartItems);
        return result;
    }
}