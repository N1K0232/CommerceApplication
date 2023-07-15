using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using FluentValidation;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveOrderValidator : AbstractValidator<SaveOrderDetail>
{
    private readonly IReadOnlyDataContext _dataContext;
    private readonly IUserClaimService _claimService;

    public SaveOrderValidator(IReadOnlyDataContext dataContext, IUserClaimService claimService)
    {
        _dataContext = dataContext;
        _claimService = claimService;
        CreateRules();
    }

    private void CreateRules()
    {
        RuleFor(o => o.OrderId).NotEmpty().Must(OrderExist).WithMessage("invalid order");
        RuleFor(o => o.ProductId).NotEmpty().Must(ProductExist).WithMessage("invalid product");
    }

    private bool OrderExist(Guid orderId)
    {
        var orderExists = _dataContext.Get<Entities.Order>()
            .Any(o => o.Id == orderId && o.UserId == _claimService.GetId());

        return orderExists;
    }

    private bool ProductExist(Guid productId)
    {
        var productExists = _dataContext.Get<Entities.Product>()
            .Any(p => p.Id == productId);

        return productExists;
    }
}