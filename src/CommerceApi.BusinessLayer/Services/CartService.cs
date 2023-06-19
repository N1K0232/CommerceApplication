using AutoMapper;
using CommerceApi.BusinessLayer.Extensions;
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
    private readonly IDataContext _dataContext;
    private readonly IAuthenticationService _authenticationService;
    private readonly IMapper _mapper;
    private readonly IUserClaimService _claimService;
    private readonly IValidator<SaveItemRequest> _itemValidator;

    public CartService(IDataContext dataContext,
        IAuthenticationService authenticationService,
        IMapper mapper,
        IUserClaimService claimService,
        IValidator<SaveItemRequest> itemValidator)
    {
        _dataContext = dataContext;
        _authenticationService = authenticationService;
        _mapper = mapper;
        _claimService = claimService;
        _itemValidator = itemValidator;
    }

    public async Task<Result> AddItemAsync(SaveItemRequest item)
    {
        try
        {
            var validationResult = await _itemValidator.ValidateAsync(item);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "Validation errors", validationErrors);
            }

            var dbItem = _mapper.Map<Entities.CartItem>(item);
            _dataContext.Create(dbItem);

            await _dataContext.SaveAsync();
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
            var query = _dataContext.GetData<Entities.Cart>();

            var dbCart = await query.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == cartId);
            if (dbCart is not null)
            {
                _dataContext.Delete(dbCart.CartItems);
                await _dataContext.SaveAsync();

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
            var userId = _claimService.GetId();
            var cartExists = await _dataContext.GetData<Entities.Cart>().AnyAsync(c => c.UserId == userId);
            if (cartExists)
            {
                return Result.Fail(FailureReasons.Conflict, "cart already exists");
            }

            var dbCart = new Entities.Cart
            {
                UserId = userId
            };

            _dataContext.Create(dbCart);
            await _dataContext.SaveAsync();

            var savedCart = _mapper.Map<Cart>(dbCart);
            savedCart.User = await _authenticationService.GetAsync();

            return savedCart;
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

        var userId = _claimService.GetId();
        var query = _dataContext.GetData<Entities.Cart>().Where(c => c.Id == cartId && c.UserId == userId);

        var cart = await query.Include(c => c.CartItems).FirstOrDefaultAsync();
        var cartItems = _mapper.Map<IEnumerable<CartItem>>(cart.CartItems);

        var result = Result<IEnumerable<CartItem>>.Ok(cartItems);
        return result;
    }

    public async Task<Result<decimal>> GetSubTotalAsync(Guid cartId)
    {
        if (cartId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var userId = _claimService.GetId();
        var query = _dataContext.GetData<Entities.Cart>();

        var cart = await query.Include(c => c.CartItems).ThenInclude(c => c.Product).FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
        var hasItems = cart?.CartItems?.Any() ?? false;
        if (!hasItems)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No item in your cart");
        }

        var subTotal = 0M;
        foreach (var item in cart.CartItems)
        {
            subTotal += (item.Product.Price * item.Quantity) + item.Product.ShippingCost.GetValueOrDefault(0M);
        }

        return subTotal;
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
            var query = _dataContext.GetData<Entities.CartItem>(trackingChanges: true);
            var cartItem = await query.FirstOrDefaultAsync(c => c.Id == itemId && c.CartId == cartId);
            if (cartItem is not null)
            {
                _dataContext.Delete(cartItem);
                await _dataContext.SaveAsync();

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, $"No item found");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}