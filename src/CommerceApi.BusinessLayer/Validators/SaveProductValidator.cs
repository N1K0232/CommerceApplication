using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class SaveProductValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductValidator()
    {
        RuleFor(p => p.CategoryId)
            .NotEmpty()
            .WithMessage("insert a valid category id");

        RuleFor(p => p.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid name");

        RuleFor(p => p.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("insert a valid description");

        RuleFor(p => p.Specifications)
            .NotNull()
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Insert a valid description");

        RuleFor(p => p.Brand)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid brand");

        RuleFor(p => p.Model)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid model");

        RuleFor(p => p.Quantity)
            .NotEmpty()
            .GreaterThanOrEqualTo(0)
            .WithMessage("insert a valid quantity");

        RuleFor(p => p.Price)
            .NotEmpty()
            .GreaterThan(0)
            .ScalePrecision(2, 5)
            .WithMessage("insert a valid price");
    }
}