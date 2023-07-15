using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result<Order>> AddDetailsAsync(SaveOrderDetail order);

    Task<Result<Order>> CreateAsync();

    Task<Result> DeleteAsync(Guid orderId);

    Task<Result<Order>> GetAsync(Guid orderId);

    Task<ListResult<Order>> GetListAsync(string orderBy, int pageIndex, int itemsPerPage);

    Task<Result<decimal>> GetTotalPriceAsync(Guid orderId);

    Task<Result<Order>> UpdateStatusAsync(UpdateOrderStatusRequest request);
}