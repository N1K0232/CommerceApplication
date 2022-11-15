using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

internal class SaveCategoryValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("Insert a valid name");
    }
}