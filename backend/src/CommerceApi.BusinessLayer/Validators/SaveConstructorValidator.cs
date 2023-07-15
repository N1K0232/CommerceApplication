using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveConstructorValidator : AbstractValidator<SaveConstructorRequest>
{
    public SaveConstructorValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100).WithMessage("Insert a valid name");
        RuleFor(c => c.Street).NotEmpty().MaximumLength(100).WithMessage("Insert a valid address");
        RuleFor(c => c.City).NotEmpty().MaximumLength(100).WithMessage("Insert a valid city");
        RuleFor(c => c.PostalCode).NotEmpty().MaximumLength(20).WithMessage("Insert a valid postal code");
    }
}