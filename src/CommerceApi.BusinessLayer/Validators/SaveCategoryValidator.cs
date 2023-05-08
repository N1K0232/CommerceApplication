using CommerceApi.Shared.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveCategoryValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryValidator()
    {
        RuleFor(c => c.Name).NotNull().WithMessage("you must insert the name");
    }
}