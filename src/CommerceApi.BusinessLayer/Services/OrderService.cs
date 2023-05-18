using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Enums;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class OrderService : IOrderService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IUserClaimService claimService;
    private readonly IValidator<SaveOrderDetail> orderValidator;

    public OrderService(IDataContext dataContext,
        IMapper mapper,
        IUserClaimService claimService,
        IValidator<SaveOrderDetail> orderValidator)
    {
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

            dataContext.Edit(dbProduct);
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
            var dbOrder = await dataContext.GetData<Entities.Order>().Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
            if (dbOrder == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No order found with id {orderId}");
            }

            dbOrder.Status = OrderStatus.Canceled;

            dataContext.Delete(dbOrder.OrderDetails);
            dataContext.Delete(dbOrder);
            await dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
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
}