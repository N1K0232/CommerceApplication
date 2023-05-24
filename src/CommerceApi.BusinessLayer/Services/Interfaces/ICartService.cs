using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface ICartService
{
    Task<Result> AddItemAsync(SaveItemRequest item);

    Task<Result> ClearCartAsync(Guid cartId);

    Task<Result<Cart>> CreateAsync();

    Task<Result<IEnumerable<CartItem>>> GetItemsAsync(Guid cartId);

    Task<Result<decimal>> GetSubTotalAsync(Guid cartId);

    Task<Result> RemoveItemAsync(Guid cartId, Guid itemId);
}