using CommerceApi.Shared.Common;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result> DeleteAsync(Guid orderId);

    Task<Result<Order>> GetAsync(Guid orderId);

    Task<ListResult<Order>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<Order>> SaveAsync(SaveOrderRequest request);
}