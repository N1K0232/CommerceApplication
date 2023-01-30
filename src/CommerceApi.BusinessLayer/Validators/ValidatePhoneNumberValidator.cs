using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class ValidatePhoneNumberValidator : AbstractValidator<ValidatePhoneNumberRequest>
{
    public ValidatePhoneNumberValidator()
    {
        RuleFor(p => p.Id)
            .NotNull()
            .NotEmpty()
            .WithMessage("the id is required");

        RuleFor(p => p.PhoneNumber)
            .NotNull()
            .NotEmpty();
    }
}