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

public class InvoiceService : IInvoiceService
{
    private readonly IDataContext dataContext;
    private readonly IMapper mapper;
    private readonly IValidator<SaveInvoiceRequest> invoiceValidator;

    public InvoiceService(IDataContext dataContext, IMapper mapper, IValidator<SaveInvoiceRequest> invoiceValidator)
    {
        this.dataContext = dataContext;
        this.mapper = mapper;
        this.invoiceValidator = invoiceValidator;
    }

    public async Task<Result<Invoice>> CreateAsync(SaveInvoiceRequest invoice)
    {
        try
        {
            var validationResult = await invoiceValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbInvoice = mapper.Map<Entities.Invoice>(invoice);
            dataContext.Create(dbInvoice);

            await dataContext.SaveAsync();

            var savedInvoice = mapper.Map<Invoice>(dbInvoice);
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

            var invoice = await dataContext.GetAsync<Entities.Invoice>(invoiceId);
            if (invoice is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
            }

            dataContext.Delete(invoice);
            await dataContext.SaveAsync();

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

        var dbInvoice = await dataContext.GetAsync<Entities.Invoice>(invoiceId);
        if (dbInvoice is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
        }


        var invoice = mapper.Map<Invoice>(dbInvoice);
        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetListAsync()
    {
        var query = dataContext.GetData<Entities.Invoice>();

        var invoices = await query.ProjectTo<Invoice>(mapper.ConfigurationProvider).ToListAsync();
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

            var validationResult = await invoiceValidator.ValidateAsync(invoice);
            if (!validationResult.IsValid)
            {
                var validationErrors = new List<ValidationError>(validationResult.Errors.Capacity);
                foreach (var error in validationResult.Errors)
                {
                    validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
                }

                return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
            }

            var dbInvoice = await dataContext.GetAsync<Entities.Invoice>(invoiceId);
            if (dbInvoice is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No invoice found for id {invoiceId}");
            }

            mapper.Map(invoice, dbInvoice);

            dataContext.Update(dbInvoice);
            await dataContext.SaveAsync();

            var savedInvoice = mapper.Map<Invoice>(dbInvoice);
            return savedInvoice;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}