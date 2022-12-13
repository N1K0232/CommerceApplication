using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("email is required");

        RuleFor(l => l.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("password is required");
    }
}