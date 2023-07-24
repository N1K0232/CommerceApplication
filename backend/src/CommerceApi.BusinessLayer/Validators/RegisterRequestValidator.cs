using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");



        RuleFor(r => r.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.ConfirmPhoneNumber)
            .NotNull()
            .NotEmpty()
            .Equal(r => r.PhoneNumber)
            .WithMessage("field required");

        RuleFor(r => r.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.ConfirmEmail)
            .NotNull()
            .NotEmpty()
            .Equal(r => r.ConfirmEmail)
            .WithMessage("field required");

        RuleFor(r => r.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("field required");

        RuleFor(r => r.ConfirmPassword)
            .NotNull()
            .NotEmpty()
            .Equal(r => r.Password)
            .WithMessage("field required");
    }
}