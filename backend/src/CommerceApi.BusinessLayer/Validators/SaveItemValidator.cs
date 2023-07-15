using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveItemValidator : AbstractValidator<SaveItemRequest>
{
    private readonly IReadOnlyDataContext _dataContext;

    public SaveItemValidator(IReadOnlyDataContext dataContext)
    {
        _dataContext = dataContext;
        CreateRules();
    }

    private void CreateRules()
    {
        RuleFor(i => i.CartId).NotEmpty().Must(CartExists).WithMessage("insert a valid cart");
        RuleFor(i => i.ProductId).NotEmpty().Must(ProductExists).WithMessage("insert a valid product");
        RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("you must add a quantity greater than 0");
    }

    private bool CartExists(Guid cartId)
    {
        var cartExists = _dataContext.Get<Cart>().Any(c => c.Id == cartId);
        return cartExists;
    }

    private bool ProductExists(Guid productId)
    {
        var productExists = _dataContext.Get<Product>().Any(p => p.Id == productId);
        return productExists;
    }
}