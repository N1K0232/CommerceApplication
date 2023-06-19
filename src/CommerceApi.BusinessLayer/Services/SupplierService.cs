using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Extensions;
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
    private readonly IDataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveSupplierRequest> _supplierValidator;

    public SupplierService(IDataContext dataContext, IMapper mapper, IValidator<SaveSupplierRequest> supplierValidator)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _supplierValidator = supplierValidator;
    }

    public async Task<Result<Supplier>> CreateAsync(SaveSupplierRequest supplier)
    {
        try
        {
            var validationResult = await _supplierValidator.ValidateAsync(supplier);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbSupplier = _mapper.Map<Entities.Supplier>(supplier);
            _dataContext.Create(dbSupplier);

            await _dataContext.SaveAsync();

            var savedSupplier = _mapper.Map<Supplier>(dbSupplier);
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

            var dbSupplier = await _dataContext.GetAsync<Entities.Supplier>(supplierId);
            if (dbSupplier is not null)
            {
                _dataContext.Delete(dbSupplier);
                await _dataContext.SaveAsync();

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

        var dbSupplier = await _dataContext.GetAsync<Entities.Supplier>(supplierId);
        if (dbSupplier is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No supplier found with id {supplierId}");
        }

        var supplier = _mapper.Map<Supplier>(dbSupplier);
        return supplier;
    }

    public async Task<IEnumerable<Supplier>> GetListAsync()
    {
        var query = _dataContext.GetData<Entities.Supplier>();

        var suppliers = await query.OrderBy(s => s.CompanyName).ProjectTo<Supplier>(_mapper.ConfigurationProvider).ToListAsync();
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

            var validationResult = await _supplierValidator.ValidateAsync(supplier);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbSupplier = await _dataContext.GetAsync<Entities.Supplier>(supplierId);
            if (dbSupplier is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No supplier found with id {supplierId}");
            }

            _mapper.Map(supplier, dbSupplier);
            _dataContext.Update(dbSupplier);

            await _dataContext.SaveAsync();

            var savedSupplier = _mapper.Map<Supplier>(dbSupplier);
            return savedSupplier;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}