using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("the firstname is required");

        RuleFor(r => r.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("the lastname is required");

        RuleFor(r => r.DateOfBirth)
            .NotNull()
            .NotEmpty()
            .WithMessage("the date of birth is required");

        RuleFor(r => r.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("the phone number is required");

        RuleFor(r => r.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("the email is required");

        RuleFor(r => r.UserName)
            .NotNull()
            .NotEmpty()
            .WithMessage("the username is required");

        RuleFor(r => r.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("the password is required");
    }
}