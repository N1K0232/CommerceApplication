using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveInvoiceValidator : AbstractValidator<SaveInvoiceRequest>
{
    private readonly IReadOnlyDataContext _dataContext;

    public SaveInvoiceValidator(IReadOnlyDataContext dataContext)
    {
        _dataContext = dataContext;
        CreateRules();
    }

    private void CreateRules()
    {
        RuleFor(i => i.ProductId).NotEmpty().Must(ProductExists).WithMessage("Insert a valid product id");
        RuleFor(i => i.Quantity).NotEmpty().GreaterThan(0).WithMessage("the quantity should be greater than 0");
        RuleFor(i => i.Price).NotEmpty().PrecisionScale(8, 2, false).WithMessage("the price is required");
    }

    private bool ProductExists(Guid productId)
    {
        var productExists = _dataContext.Get<Product>().Any(p => p.Id == productId);
        return productExists;
    }
}