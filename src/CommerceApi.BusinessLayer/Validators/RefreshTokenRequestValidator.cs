using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(r => r.AccessToken)
            .NotNull()
            .NotEmpty()
            .WithMessage("the access token is required");

        RuleFor(r => r.RefreshToken)
            .NotNull()
            .NotEmpty()
            .WithMessage("the refresh token is required");
    }
}