using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class SaveOrderValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderValidator()
    {
        RuleFor(o => o.ProductId)
            .NotEmpty()
            .WithMessage("the product is required");

        RuleFor(o => o.OrderedQuantity)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("a valid quantity is required");
    }
}