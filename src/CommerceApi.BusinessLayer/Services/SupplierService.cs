using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
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

    public async Task<Result<Supplier>> CreateAsync(SaveSupplierRequest supplier)
    {
        try
        {
            var validationResult = await supplierValidator.ValidateAsync(supplier);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbSupplier = mapper.Map<Entities.Supplier>(supplier);
            dataContext.Create(dbSupplier);

            await dataContext.SaveAsync();

            var savedSupplier = mapper.Map<Supplier>(dbSupplier);
            return savedSupplier;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid supplierId)
    {
        try
        {
            if (supplierId == Guid.Empty)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid id");
            }

            var dbSupplier = await dataContext.GetAsync<Entities.Supplier>(supplierId);
            if (dbSupplier is not null)
            {
                dataContext.Delete(dbSupplier);
                await dataContext.SaveAsync();

                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ItemNotFound, $"No supplier found with id {supplierId}");
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Supplier>> GetAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var dbSupplier = await dataContext.GetAsync<Entities.Supplier>(supplierId);
        if (dbSupplier is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No supplier found with id {supplierId}");
        }

        var supplier = mapper.Map<Supplier>(dbSupplier);
        return supplier;
    }

    public async Task<IEnumerable<Supplier>> GetListAsync()
    {
        var query = dataContext.GetData<Entities.Supplier>();

        var suppliers = await query.OrderBy(s => s.CompanyName).ProjectTo<Supplier>(mapper.ConfigurationProvider).ToListAsync();
        return suppliers;
    }

    public async Task<Result<Supplier>> UpdateAsync(Guid supplierId, SaveSupplierRequest supplier)
    {
        try
        {
            if (supplierId == Guid.Empty)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid id");
            }

            var validationResult = await supplierValidator.ValidateAsync(supplier);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbSupplier = await dataContext.GetAsync<Entities.Supplier>(supplierId);
            if (dbSupplier is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No supplier found with id {supplierId}");
            }

            mapper.Map(supplier, dbSupplier);
            dataContext.Edit(dbSupplier);

            await dataContext.SaveAsync();

            var savedSupplier = mapper.Map<Supplier>(dbSupplier);
            return savedSupplier;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}