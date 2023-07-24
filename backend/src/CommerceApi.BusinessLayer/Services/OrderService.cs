﻿using System.Linq.Dynamic.Core;
using AutoMapper;
using CommerceApi.BusinessLayer.Extensions;
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
    private readonly IAuthenticatedService _authenticationService;
    private readonly IDataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IUserClaimService _claimService;
    private readonly IValidator<SaveOrderDetail> _orderValidator;

    public OrderService(IAuthenticatedService authenticationService,
        IDataContext dataContext,
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
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var products = _dataContext.Get<Entities.Product>(trackingChanges: true);
            var orders = _dataContext.Get<Entities.Order>();

            await _dataContext.ExecuteTransactionAsync(async () =>
            {
                var dbProduct = await products.FirstAsync(p => p.Id == order.ProductId);
                var dbOrder = await orders.FirstAsync(o => o.Id == order.OrderId && o.UserId == _claimService.GetId());

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
            });

            var dbOrder = await orders.FirstOrDefaultAsync(o => o.Id == order.OrderId);

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
            var random = new Random();
            var dbOrder = new Entities.Order
            {
                UserId = _claimService.GetId(),
                IdentificationNumber = random.NextInt64(1, long.MaxValue).ToString(),
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
            var query = _dataContext.Get<Entities.Order>();
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

        var query = _dataContext.Get<Entities.Order>();
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
        var query = _dataContext.Get<Entities.Order>();
        var userId = _claimService.GetId();

        if (userId != Guid.Empty)
        {
            query = query.Where(o => o.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        var dbOrders = await query.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1).ToListAsync();

        var orders = _mapper.Map<IEnumerable<Order>>(dbOrders).Take(itemsPerPage);
        await orders.ForEachAsync(async (order) =>
        {
            var products = await GetProductsAsync(order.Id);
            order.Products = products;
        });

        var totalCount = await query.LongCountAsync();
        var totalPages = totalCount / itemsPerPage;
        var hasNextPage = dbOrders.Count > itemsPerPage;

        var result = new ListResult<Order>(orders, totalCount, totalPages, hasNextPage);
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

        var query = _dataContext.Get<Entities.Order>();
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
            var query = _dataContext.Get<Entities.Order>(trackingChanges: true);
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
        var query = _dataContext.Get<Entities.OrderDetail>();
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