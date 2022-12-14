using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class SupplierService : ISupplierService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveSupplierRequest> supplierValidator;

    public SupplierService(IDataContext dataContext, IMapper mapper, IValidator<SaveSupplierRequest> supplierValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.supplierValidator = supplierValidator;
    }


    public async Task<Result> DeleteAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var supplier = await dataContext.GetAsync<Entities.Supplier>(supplierId);
        if (supplier is not null)
        {
            dataContext.Delete(supplier);

            var deletedEntries = await dataContext.SaveAsync();
            if (deletedEntries > 0)
            {
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.DatabaseError, "can't delete the supplier");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no supplier found");
    }

    public async Task<IEnumerable<Supplier>> GetAsync()
    {
        var suppliers = await dataContext.GetData<Entities.Supplier>()
            .ProjectTo<Supplier>(mapper.ConfigurationProvider)
            .ToListAsync();

        return suppliers;
    }

    public async Task<Result<Supplier>> GetAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        var supplier = await dataContext.GetData<Entities.Supplier>()
            .ProjectTo<Supplier>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(s => s.Id == supplierId);

        if (supplier is not null)
        {
            return supplier;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no supplier found");
    }

    public async Task<Result<Supplier>> SaveAsync(SaveSupplierRequest request)
    {
        var validationResult = await supplierValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var validationErrors = new List<ValidationError>();
            foreach (var error in validationResult.Errors)
            {
                validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
            }

            return Result.Fail(FailureReasons.GenericError, "Invalid request", validationErrors);
        }

        var supplier = request.Id != null ? await dataContext.GetData<Entities.Supplier>(trackingChanges: true)
            .FirstOrDefaultAsync(s => s.Id == request.Id) : null;

        if (supplier is null)
        {
            supplier = mapper.Map<Entities.Supplier>(request);

            var supplierExists = await dataContext.ExistsAsync<Entities.Supplier>(s =>
                s.CompanyName == supplier.CompanyName &&
                s.ContactName == supplier.ContactName &&
                s.City == supplier.City);

            if (supplierExists)
            {
                return Result.Fail(FailureReasons.Conflict, "the supplier already exists");
            }

            dataContext.Create(supplier);
        }
        else
        {
            mapper.Map(request, supplier);
            dataContext.Edit(supplier);
        }

        var savedEntries = await dataContext.SaveAsync();
        if (savedEntries > 0)
        {
            var savedSupplier = mapper.Map<Supplier>(supplier);
            return savedSupplier;
        }

        return Result.Fail(FailureReasons.DatabaseError, "Can't save supplier");
    }
}