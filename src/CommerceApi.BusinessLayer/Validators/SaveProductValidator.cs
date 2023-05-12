using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveProductValidator : AbstractValidator<SaveProductRequest>
{
    private readonly IReadOnlyDataContext dataContext;

    public SaveProductValidator(IReadOnlyDataContext dataContext)
    {
        this.dataContext = dataContext;
        CreateValidationRules();
    }

    private void CreateValidationRules()
    {
        RuleFor(p => p.CategoryId).NotEmpty().Must(ExistsCategory).WithMessage("Insert a valid category");
        RuleFor(p => p.SupplierId).NotEmpty().Must(ExistsSupplier).WithMessage("Insert a valid supplier");
        RuleFor(p => p.Name).NotNull().NotEmpty().MaximumLength(100).WithMessage("Insert a valid name");
        RuleFor(p => p.Price).GreaterThan(0).PrecisionScale(8, 2, false).WithMessage("Insert a valid price");
        RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0).WithMessage("you can't add a product with negative quantity");
    }

    private bool ExistsCategory(Guid categoryId)
    {
        var categoryExists = dataContext.GetData<Category>().Any(c => c.Id == categoryId);
        return categoryExists;
    }

    private bool ExistsSupplier(Guid supplierId)
    {
        var supplierExists = dataContext.GetData<Supplier>().Any(s => s.Id == supplierId);
        return supplierExists;
    }
}