using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveCouponValidator : AbstractValidator<SaveCouponRequest>
{
    private static readonly DateTime now = DateTime.UtcNow;

    public SaveCouponValidator()
    {
        RuleFor(c => c.Code).NotNull().NotEmpty().WithMessage("the code is required");
        RuleFor(c => c.DiscountPercentage).NotEqual(0F).WithMessage("insert a valid discount");

        RuleFor(c => c.StartDate)
            .NotEqual(DateTime.MinValue)
            .InclusiveBetween(now, now.AddMonths(6))
            .WithMessage("insert a valid start date");

        RuleFor(c => c.ExpirationDate)
            .NotEqual(DateTime.MinValue)
            .GreaterThan(c => c.StartDate)
            .InclusiveBetween(now.AddDays(1), now.AddMonths(6))
            .WithMessage("insert a valid expiration date");
    }
}