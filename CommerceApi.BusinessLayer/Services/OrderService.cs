using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
using CommerceApi.Shared.Common;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IAuthenticationService authenticationService;
    private readonly IValidator<SaveOrderRequest> orderValidator;

    private readonly Guid userId;

    public OrderService(IDataContext dataContext,
        IMapper mapper,
        IAuthenticationService authenticationService,
        IUserClaimService claimService,
        IValidator<SaveOrderRequest> orderValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.authenticationService = authenticationService;
        this.orderValidator = orderValidator;

        userId = claimService.GetId();
    }


    public async Task<Result> DeleteAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var order = await dataContext.GetData<Entities.Order>().FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is not null)
        {
            order.Status = OrderStatus.Canceled;
            dataContext.Delete(order);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "Cannot delete order");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<Result<Order>> GetAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var dbOrder = await dataContext.GetData<Entities.Order>(ignoreAutoIncludes: false)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId);

        if (dbOrder is not null)
        {
            var products = new List<Product>();

            foreach (var detail in dbOrder.OrderDetails)
            {
                var product = mapper.Map<Product>(detail.Product);
                products.Add(product);
            }

            var order = mapper.Map<Order>(dbOrder);
            order.Products = products;

            return order;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<ListResult<Order>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dataContext.GetData<Entities.Order>(ignoreAutoIncludes: false);

        var totalCount = await query.CountAsync();
        var dbOrders = await query.Include(o => o.OrderDetails)
            .OrderBy(orderBy)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ToListAsync();

        var orders = new List<Order>(dbOrders.Count);

        foreach (var dbOrder in dbOrders)
        {
            var order = mapper.Map<Order>(dbOrder);
            order.User = await authenticationService.GetAsync(userId);

            var products = new List<Product>();

            foreach (var detail in dbOrder.OrderDetails)
            {
                var product = await dataContext.GetData<Entities.Product>()
                    .Include(p => p.Category)
                    .ProjectTo<Product>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(p => p.Id == detail.ProductId);

                products.Add(product);
            }

            order.Products = products;
            orders.Add(order);
        }

        var result = new ListResult<Order>(orders.Take(itemsPerPage), totalCount, orders.Count > itemsPerPage);
        return result;
    }

    public async Task<Result<Order>> SaveAsync(SaveOrderRequest request)
    {
        var validationResult = await orderValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var productExists = await dataContext.ExistsAsync<Entities.Product>(request.ProductId);
        if (!productExists)
        {
            return Result.Fail(FailureReasons.GenericError, "the product doesn't exist");
        }

        var product = await dataContext.GetAsync<Entities.Product>(request.ProductId);

        var query = dataContext.GetData<Entities.Order>(ignoreAutoIncludes: false, ignoreQueryFilters: true, trackingChanges: true);
        var order = request.Id != null ? await query.FirstOrDefaultAsync(o => o.Id == request.Id) : null;

        if (order is null)
        {
            var now = DateTime.UtcNow;

            order = new Entities.Order
            {
                UserId = userId,
                Status = OrderStatus.New,
                OrderDate = now,
                OrderTime = new TimeSpan(now.Hour, now.Minute, now.Second)
            };

            order.OrderDetails.Add(new Entities.OrderDetail
            {
                OrderId = order.Id,
                ProductId = product.Id,
                OrderedQuantity = request.OrderedQuantity,
                Price = product.Price
            });

            if (product.Quantity > request.OrderedQuantity)
            {
                product.Quantity -= request.OrderedQuantity;
                dataContext.Edit(product);
            }
            else
            {
                return Result.Fail(FailureReasons.GenericError, "There aren't enough products");
            }

            dataContext.Create(order);
        }
        else
        {
            var orderDetail = order.OrderDetails.FirstOrDefault(o => o.OrderId == order.Id);
            order.OrderDetails.Remove(orderDetail);

            orderDetail.OrderId = order.Id;
            orderDetail.ProductId = product.Id;
            orderDetail.Price = product.Price;
            orderDetail.OrderedQuantity = request.OrderedQuantity;

            order.OrderDetails.Add(orderDetail);
            order.Status = request.Status.GetValueOrDefault(OrderStatus.InProgress);

            dataContext.Edit(order);
        }

        try
        {
            await dataContext.ExecuteTransactionAsync();

            var user = await authenticationService.GetAsync(userId);

            var savedOrder = mapper.Map<Order>(order);
            savedOrder.User = user;
            return savedOrder;
        }
        catch (Exception ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}