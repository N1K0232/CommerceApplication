using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result<Order>> AddDetailsAsync(SaveOrderDetail order);

    Task<Result<Order>> CreateAsync();

    Task<Result> DeleteAsync(Guid orderId);

    Task<Result<decimal>> GetTotalPriceAsync(Guid orderId);
}