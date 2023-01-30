using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class ValidateEmailValidator : AbstractValidator<ValidateEmailRequest>
{
    public ValidateEmailValidator()
    {
        RuleFor(e => e.Id)
            .NotNull()
            .NotEmpty()
            .WithMessage("the id is required");

        RuleFor(e => e.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty()
            .Length(6, 100)
            .WithMessage("the email is invalid");
    }
}