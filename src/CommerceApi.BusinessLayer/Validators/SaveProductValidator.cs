using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Entities;
using CommerceApi.Shared.Models.Requests;
using FluentValidation;

namespace CommerceApi.BusinessLayer.Validators;

public class SaveProductValidator : AbstractValidator<SaveProductRequest>
{
    private readonly IReadOnlyDataContext _dataContext;

    public SaveProductValidator(IReadOnlyDataContext dataContext)
    {
        _dataContext = dataContext;
        CreateValidationRules();
    }

    private void CreateValidationRules()
    {
        RuleFor(p => p.CategoryId).NotEmpty().Must(ExistsCategory).WithMessage("Insert a valid category");
        RuleFor(p => p.ConstructorId).NotEmpty().Must(ExistsConstructor).WithMessage("Insert a valid constructor");
        RuleFor(p => p.SupplierId).NotEmpty().Must(ExistsSupplier).WithMessage("Insert a valid supplier");
        RuleFor(p => p.Name).NotNull().NotEmpty().MaximumLength(256).WithMessage("Insert a valid name");
        RuleFor(p => p.Description).NotNull().NotEmpty().MaximumLength(4000).WithMessage("description is required");
        RuleFor(p => p.Price).GreaterThan(0).PrecisionScale(8, 2, false).WithMessage("Insert a valid price");
        RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0).WithMessage("you can't add a product with negative quantity");
    }

    private bool ExistsCategory(Guid categoryId)
    {
        var query = _dataContext.GetData<Category>();

        var categoryExists = query.Any(c => c.Id == categoryId);
        return categoryExists;
    }

    private bool ExistsConstructor(Guid constructorId)
    {
        var query = _dataContext.GetData<Constructor>();

        var constructorExists = query.Any(c => c.Id == constructorId);
        return constructorExists;
    }

    private bool ExistsSupplier(Guid supplierId)
    {
        var query = _dataContext.GetData<Supplier>();

        var supplierExists = query.Any(s => s.Id == supplierId);
        return supplierExists;
    }
}