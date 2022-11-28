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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IMemoryCache cache;
    private readonly IValidator<SaveOrderRequest> orderValidator;
    private readonly ILogger<OrderService> logger;

    private readonly Guid userId;

    public OrderService(IDataContext dataContext,
        IMapper mapper,
        IMemoryCache cache,
        IUserClaimService claimService,
        IValidator<SaveOrderRequest> orderValidator,
        ILogger<OrderService> logger)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.cache = cache;
        this.orderValidator = orderValidator;
        this.logger = logger;

        userId = claimService.GetId();
    }


    public async Task<Result> DeleteAsync(Guid orderId)
    {
        logger.LogInformation("deleting order");

        if (orderId == Guid.Empty)
        {
            logger.LogError("Invalid id", orderId);
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
                logger.LogInformation("order successfully deleted");
                return Result.Ok();
            }

            logger.LogError("Cannot delete order");
            return Result.Fail(FailureReasons.DatabaseError, "Cannot delete order");
        }

        logger.LogError("No order found");
        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<Result<Order>> GetAsync(Guid orderId)
    {
        logger.LogInformation("get the single order");

        if (orderId == Guid.Empty)
        {
            logger.LogError("Invalid id", orderId);
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

        logger.LogError("No order found");
        return Result.Fail(FailureReasons.ItemNotFound, "No order found");
    }

    public async Task<ListResult<Order>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy)
    {
        logger.LogInformation("get the list of orders");

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

            var user = GetUser();
            order.User = user;

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

        var result = new ListResult<Order>();
        result.Content = orders.Take(itemsPerPage);
        result.TotalCount = totalCount;
        result.HasNextPage = orders.Count > itemsPerPage;

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

            logger.LogError("Invalid request", validationErrors);
            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var productExists = await dataContext.ExistsAsync<Entities.Product>(request.ProductId);
        if (!productExists)
        {
            logger.LogError("Invalid product", request.ProductId);
            return Result.Fail(FailureReasons.GenericError, "the product doesn't exist");
        }

        var product = await dataContext.GetAsync<Entities.Product>(request.ProductId);

        var query = dataContext.GetData<Entities.Order>(ignoreAutoIncludes: false, ignoreQueryFilters: true, trackingChanges: true);
        var order = request.Id != null ? await query.FirstOrDefaultAsync(o => o.Id == request.Id) : null;

        if (order is null)
        {
            logger.LogInformation("saving new order");

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
            logger.LogInformation("updating order");

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

            logger.LogInformation("successfully saved order");
            var user = GetUser();

            var savedOrder = mapper.Map<Order>(order);
            savedOrder.User = user;

            return savedOrder;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "cannot save order", request);
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    private User GetUser()
    {
        var user = (User)cache.Get("AuthenticatedUser");
        return user;
    }
}