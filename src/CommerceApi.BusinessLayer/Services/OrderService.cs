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
    private readonly IAuthenticationService authenticationService;
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IUserClaimService claimService;
    private readonly IValidator<SaveOrderDetail> orderValidator;

    public OrderService(IAuthenticationService authenticationService,
        IDataContext dataContext,
        IMapper mapper,
        IUserClaimService claimService,
        IValidator<SaveOrderDetail> orderValidator)
    {
        this.authenticationService = authenticationService;
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.claimService = claimService;
        this.orderValidator = orderValidator;
    }

    public async Task<Result<Order>> AddDetailsAsync(SaveOrderDetail order)
    {
        try
        {
            var validationResult = await orderValidator.ValidateAsync(order);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }


            var dbProduct = await dataContext.GetData<Entities.Product>(trackingChanges: true).FirstOrDefaultAsync(p => p.Id == order.ProductId);
            var dbOrder = await dataContext.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.Id == order.OrderId && o.UserId == claimService.GetId());

            var orderDetail = new Entities.OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                UnitPrice = dbProduct.Price
            };

            dbProduct.Quantity -= order.Quantity;

            dataContext.Update(dbProduct);
            dataContext.Create(orderDetail);
            await dataContext.SaveAsync();

            var savedOrder = mapper.Map<Order>(dbOrder);
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
                UserId = claimService.GetId(),
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Time = TimeOnly.FromDateTime(DateTime.UtcNow),
                Status = OrderStatus.New,
            };

            dataContext.Create(dbOrder);
            await dataContext.SaveAsync();

            var savedOrder = mapper.Map<Order>(dbOrder);
            var user = await authenticationService.GetAsync();

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
            var dbOrder = await dataContext.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.Id == orderId);
            if (dbOrder == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
            }

            var dbOrderDetails = await dataContext.GetData<Entities.OrderDetail>().Where(o => o.OrderId == orderId).ToListAsync();

            dbOrder.Status = OrderStatus.Canceled;

            if (dbOrderDetails.Any())
            {
                dataContext.Delete(dbOrderDetails);
            }

            dataContext.Delete(dbOrder);
            await dataContext.SaveAsync();

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

        var dbOrder = await dataContext.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.Id == orderId);
        if (dbOrder == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
        }

        var products = await GetProductsAsync(orderId);

        var order = mapper.Map<Order>(dbOrder);
        order.Products = products;

        return order;
    }

    public async Task<ListResult<Order>> GetListAsync(string orderBy, int pageIndex, int itemsPerPage)
    {
        var query = dataContext.GetData<Entities.Order>();
        var userId = claimService.GetId();

        query.Where(o => o.UserId == userId);

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var totalCount = await query.LongCountAsync();

        var dbOrders = await query.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1).ToListAsync();
        var orders = mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);

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

        var query = dataContext.GetData<Entities.Order>();
        var dbOrder = await query.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == claimService.GetId());
        if (dbOrder == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
        }

        foreach (var orderDetail in dbOrder.OrderDetails)
        {
            totalPrice += orderDetail.UnitPrice * orderDetail.Quantity;
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
            var dbOrder = await dataContext.GetData<Entities.Order>(trackingChanges: true).FirstOrDefaultAsync(o => o.Id == request.OrderId);
            if (dbOrder is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, "No order found");
            }

            if (dbOrder.Status == request.Status || request.Status == OrderStatus.New || request.Status == OrderStatus.Canceled)
            {
                return Result.Fail(FailureReasons.GenericError, "you can't update the status of the order");
            }

            dbOrder.Status = request.Status;
            dataContext.Update(dbOrder);
            await dataContext.SaveAsync();

            var savedOrder = mapper.Map<Order>(dbOrder);
            return savedOrder;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    private async Task<IEnumerable<Product>> GetProductsAsync(Guid orderId)
    {
        var hasDetails = await dataContext.GetData<Entities.OrderDetail>().AnyAsync(o => o.OrderId == orderId);
        if (!hasDetails)
        {
            return null;
        }

        var orderDetails = await dataContext.GetData<Entities.OrderDetail>().Include(o => o.Product).Where(o => o.OrderId == orderId).ToListAsync();
        var products = new List<Product>(orderDetails.Count);
        foreach (var orderDetail in orderDetails)
        {
            var product = mapper.Map<Product>(orderDetail.Product);
            products.Add(product);
        }

        return products;
    }
}