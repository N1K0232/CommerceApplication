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

public class ConstructorService : IConstructorService
{
    private readonly IDataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveConstructorRequest> _constructorValidator;

    public ConstructorService(IDataContext dataContext, IMapper mapper, IValidator<SaveConstructorRequest> constructorValidator)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _constructorValidator = constructorValidator;
    }

    public async Task<Result<Constructor>> CreateAsync(SaveConstructorRequest constructor)
    {
        var validationResult = await _constructorValidator.ValidateAsync(constructor);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var query = _dataContext.Get<Entities.Constructor>();
            var constructorExists = await query.AnyAsync(c => c.Name == constructor.Name);
            if (constructorExists)
            {
                return Result.Fail(FailureReasons.Conflict, "constructor already exists");
            }

            var dbConstructor = _mapper.Map<Entities.Constructor>(constructor);
            _dataContext.Create(dbConstructor);
            await _dataContext.SaveAsync();

            var savedConstructor = _mapper.Map<Constructor>(dbConstructor);
            return savedConstructor;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid constructorId)
    {
        if (constructorId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        try
        {
            var dbConstructor = await _dataContext.GetAsync<Entities.Constructor>(constructorId);
            if (dbConstructor == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No constructor found with id {constructorId}");
            }

            _dataContext.Delete(dbConstructor);
            await _dataContext.SaveAsync();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<Constructor>> GetAsync(Guid constructorId)
    {
        if (constructorId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var dbConstructor = await _dataContext.GetAsync<Entities.Constructor>(constructorId);
        if (dbConstructor == null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No constructor found with id {constructorId}");
        }

        var constructor = _mapper.Map<Constructor>(dbConstructor);
        return constructor;
    }

    public async Task<IEnumerable<Constructor>> GetListAsync(string city)
    {
        var query = _dataContext.Get<Entities.Constructor>();
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.City.Contains(city));
        }

        var constructors = await query.OrderBy(c => c.Name)
            .ProjectTo<Constructor>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return constructors;
    }

    public async Task<Result<Constructor>> UpdateAsync(Guid constructorId, SaveConstructorRequest constructor)
    {
        if (constructorId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var validationResult = await _constructorValidator.ValidateAsync(constructor);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var query = _dataContext.Get<Entities.Constructor>(trackingChanges: true);
            var dbConstructor = await query.FirstOrDefaultAsync(c => c.Id == constructorId);

            if (dbConstructor == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No constructor found with id {constructorId}");
            }

            _mapper.Map(constructor, dbConstructor);
            _dataContext.Update(dbConstructor);
            await _dataContext.SaveAsync();

            var savedConstructor = _mapper.Map<Constructor>(dbConstructor);
            return savedConstructor;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }
}