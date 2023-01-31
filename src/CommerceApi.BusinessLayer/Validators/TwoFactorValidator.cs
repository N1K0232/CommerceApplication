using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class TwoFactorValidator : AbstractValidator<TwoFactorRequest>
{
    public TwoFactorValidator()
    {
        RuleFor(t => t.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty()
            .Length(6, 100)
            .WithMessage("Insert a valid email");
    }
}