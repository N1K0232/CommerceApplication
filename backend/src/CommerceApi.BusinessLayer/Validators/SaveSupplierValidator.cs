using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveSupplierValidator : AbstractValidator<SaveSupplierRequest>
{
    public SaveSupplierValidator()
    {
        RuleFor(s => s.CompanyName).NotNull().NotEmpty().MaximumLength(100).WithMessage("the company name is required");
        RuleFor(s => s.ContactName).NotNull().NotEmpty().MaximumLength(100).WithMessage("the contact name is required");
        RuleFor(s => s.City).NotNull().NotEmpty().MaximumLength(100).WithMessage("the city is required");
    }
}