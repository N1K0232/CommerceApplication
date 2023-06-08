using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveReviewValidator : AbstractValidator<SaveReviewRequest>
{
    private readonly IReadOnlyDataContext _dataContext;

    public SaveReviewValidator(IReadOnlyDataContext dataContext)
    {
        _dataContext = dataContext;
        CreateValidationRules();
    }

    private void CreateValidationRules()
    {
        RuleFor(r => r.ProductId).NotEmpty().Must(ProductExists).WithMessage("invalid product");
        RuleFor(r => r.Text).NotNull().NotEmpty().MaximumLength(4000).WithMessage("please insert a valid review");
        RuleFor(r => r.Score).InclusiveBetween(1, 5).WithMessage("please insert a valid score between 1 and 5");
    }

    private bool ProductExists(Guid productId)
    {
        var query = _dataContext.GetData<Product>();

        var productExists = query.Any(p => p.Id == productId);
        return productExists;
    }
}