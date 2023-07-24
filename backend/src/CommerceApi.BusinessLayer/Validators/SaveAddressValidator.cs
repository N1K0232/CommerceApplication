using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveAddressValidator : AbstractValidator<SaveAddressRequest>
{
    public SaveAddressValidator()
    {
        RuleFor(r => r.Street)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.City)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.PostalCode)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.Country)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");
    }
}