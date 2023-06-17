using System.Linq.Dynamic.Core;
using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Common;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICommerceApplicationDbContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IUserClaimService _claimService;
    private readonly IValidator<SaveOrderDetail> _orderValidator;

    public OrderService(IAuthenticationService authenticationService,
        ICommerceApplicationDbContext dataContext,
        IMapper mapper,
        IUserClaimService claimService,
        IValidator<SaveOrderDetail> orderValidator)
    {
        _authenticationService = authenticationService;
        _dataContext = dataContext;
        _mapper = mapper;
        _claimService = claimService;
        _orderValidator = orderValidator;
    }

    public async Task<Result<Order>> AddDetailsAsync(SaveOrderDetail order)
    {
        try
        {
            var validationResult = await _orderValidator.ValidateAsync(order);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }


            var dbProduct = await _dataContext.GetData<Entities.Product>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == order.ProductId);
            var dbOrder = await _dataContext.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.Id == order.OrderId && o.UserId == _claimService.GetId());

            var orderDetail = new Entities.OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                UnitPrice = dbProduct.Price
            };

            dbProduct.Quantity -= order.Quantity;

            _dataContext.Update(dbProduct);
            _dataContext.Create(orderDetail);
            await _dataContext.SaveAsync();

            var savedOrder = _mapper.Map<Order>(dbOrder);
            return savedOrder;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Order>> CreateAsync()
    {
        try
        {
            var dbOrder = new Entities.Order
            {
                UserId = _claimService.GetId(),
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Time = TimeOnly.FromDateTime(DateTime.UtcNow),
                Status = OrderStatus.New,
            };

            _dataContext.Create(dbOrder);
            await _dataContext.SaveAsync();

            var savedOrder = _mapper.Map<Order>(dbOrder);
            var user = await _authenticationService.GetAsync();

            savedOrder.User = $"{user.FirstName} {user.LastName}";
            return savedOrder;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        try
        {
            var query = _dataContext.GetData<Entities.Order>();
            var dbOrder = await query.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
            if (dbOrder == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
            }

            dbOrder.Status = OrderStatus.Canceled;
            var dbOrderDetails = dbOrder.OrderDetails;

            if (dbOrderDetails.Any())
            {
                _dataContext.Delete(dbOrderDetails);
            }

            _dataContext.Delete(dbOrder);
            await _dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Order>> GetAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var query = _dataContext.GetData<Entities.Order>();
        var dbOrder = await query.FirstOrDefaultAsync(o => o.Id == orderId);
        if (dbOrder == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
        }

        var order = _mapper.Map<Order>(dbOrder);
        order.Products = await GetProductsAsync(orderId);

        return order;
    }

    public async Task<ListResult<Order>> GetListAsync(string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = _dataContext.GetData<Entities.Order>();
        var userId = _claimService.GetId();

        if (userId != Guid.Empty)
        {
            query = query.Where(o => o.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var totalCount = await query.LongCountAsync();
        var dbOrders = await query.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1).ToListAsync();

        var orders = _mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        await orders.ForEachAsync(async (order) =>
        {
            var products = await GetProductsAsync(order.Id);
            order.Products = products;
        });

        var result = new ListResult<Order>(orders, totalCount, dbOrders.Count > itemsPerPage);
        return result;
    }

    public async Task<Result<decimal>> GetTotalPriceAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var totalPrice = 0M;
        var userId = _claimService.GetId();

        var query = _dataContext.GetData<Entities.Order>();
        var order = await query.Include(o => o.OrderDetails).ThenInclude(o => o.Product).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        var hasItems = order?.OrderDetails?.Any() ?? false;
        if (!hasItems)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
        }

        foreach (var detail in order.OrderDetails)
        {
            totalPrice += (detail.UnitPrice * detail.Quantity) + detail.Product.ShippingCost.GetValueOrDefault(0M);
        }

        return totalPrice;
    }

    public async Task<Result<Order>> UpdateStatusAsync(UpdateOrderStatusRequest request)
    {
        if (request.OrderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var query = _dataContext.GetData<Entities.Order>(trackingChanges: true);
            var dbOrder = await query.FirstOrDefaultAsync(o => o.Id == request.OrderId);
            if (dbOrder is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, "No order found");
            }

            if (dbOrder.Status == request.Status || request.Status == OrderStatus.New || request.Status == OrderStatus.Canceled)
            {
                return Result.Fail(FailureReasons.GenericError, "you can't update the status of the order");
            }

            dbOrder.Status = request.Status;

            _dataContext.Update(dbOrder);
            await _dataContext.SaveAsync();

            var savedOrder = _mapper.Map<Order>(dbOrder);
            return savedOrder;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    private async Task<IEnumerable<Product>> GetProductsAsync(Guid orderId)
    {
        var query = _dataContext.GetData<Entities.OrderDetail>();
        var hasDetails = await query.AnyAsync(o => o.OrderId == orderId);
        if (!hasDetails)
        {
            return null;
        }

        var orderDetails = await query.Include(o => o.Product).Where(o => o.OrderId == orderId).ToListAsync();
        var products = new List<Product>(orderDetails.Count);
        foreach (var orderDetail in orderDetails)
        {
            var product = _mapper.Map<Product>(orderDetail.Product);
            products.Add(product);
        }

        return products;
    }
}