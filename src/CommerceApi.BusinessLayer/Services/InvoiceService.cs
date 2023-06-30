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

public class InvoiceService : IInvoiceService
{
    private readonly IDataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveInvoiceRequest> _invoiceValidator;

    public InvoiceService(IDataContext dataContext, IMapper mapper, IValidator<SaveInvoiceRequest> invoiceValidator)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _invoiceValidator = invoiceValidator;
    }

    public async Task<Result<Invoice>> CreateAsync(SaveInvoiceRequest invoice)
    {
        try
        {
            var validationResult = await _invoiceValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbInvoice = _mapper.Map<Entities.Invoice>(invoice);
            _dataContext.Create(dbInvoice);

            await _dataContext.SaveAsync();

            var savedInvoice = _mapper.Map<Invoice>(dbInvoice);
            return savedInvoice;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid invoiceId)
    {
        try
        {
            if (invoiceId == Guid.Empty)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid id");
            }

            var invoice = await _dataContext.GetAsync<Entities.Invoice>(invoiceId);
            if (invoice is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
            }

            _dataContext.Delete(invoice);
            await _dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Invoice>> GetAsync(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        var dbInvoice = await _dataContext.GetAsync<Entities.Invoice>(invoiceId);
        if (dbInvoice is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
        }


        var invoice = _mapper.Map<Invoice>(dbInvoice);
        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetListAsync()
    {
        var query = _dataContext.Get<Entities.Invoice>();

        var invoices = await query.ProjectTo<Invoice>(_mapper.ConfigurationProvider).ToListAsync();
        return invoices;
    }

    public async Task<Result<Invoice>> UpdateAsync(Guid invoiceId, SaveInvoiceRequest invoice)
    {
        try
        {
            if (invoiceId == Guid.Empty)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid id");
            }

            var validationResult = await _invoiceValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.ToValidationErrors();
                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbInvoice = await _dataContext.GetAsync<Entities.Invoice>(invoiceId);
            if (dbInvoice is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
            }

            _mapper.Map(invoice, dbInvoice);

            _dataContext.Update(dbInvoice);
            await _dataContext.SaveAsync();

            var savedInvoice = _mapper.Map<Invoice>(dbInvoice);
            return savedInvoice;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}