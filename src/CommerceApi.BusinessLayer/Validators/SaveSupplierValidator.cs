using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class SaveSupplierValidator : AbstractValidator<SaveSupplierRequest>
{
    public SaveSupplierValidator()
    {
        RuleFor(s => s.CompanyName)
            .MaximumLength(256)
            .NotNull()
            .NotEmpty()
            .WithMessage("insert a valid company name");

        RuleFor(s => s.ContactName)
            .MaximumLength(256)
            .NotNull()
            .NotEmpty()
            .WithMessage("insert a valid contact name");

        RuleFor(s => s.City)
            .MaximumLength(100)
            .NotNull()
            .NotEmpty()
            .WithMessage("insert a valid city");
    }
}