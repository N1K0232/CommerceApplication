using System.Security.Claims;
using AutoMapper;
using CommerceApi.Authentication.Managers;
using CommerceApi.BusinessLayer.Extensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using CommerceApi.SharedServices;
using CommerceApi.StorageProviders.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using Entities = CommerceApi.Authentication.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class AuthenticatedService : IAuthenticatedService
{
    private readonly ApplicationUserManager _userManager;
    private readonly IStorageProvider _storageProvider;
    private readonly IUserClaimService _claimService;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveAddressRequest> _addressValidator;

    public AuthenticatedService(ApplicationUserManager userManager,
        IStorageProvider storageProvider,
        IUserClaimService claimService,
        IMapper mapper,
        IValidator<SaveAddressRequest> addressValidator)
    {
        _userManager = userManager;
        _storageProvider = storageProvider;
        _claimService = claimService;
        _mapper = mapper;
        _addressValidator = addressValidator;
    }


    public async Task<User> GetAsync()
    {
        var userId = _claimService.GetId();

        var dbUser = await _userManager.FindByIdAsync(userId.ToString());
        var userRoles = await _userManager.GetRolesAsync(dbUser);
        var addresses = await _userManager.GetAddressesAsync(dbUser);

        var user = _mapper.Map<User>(dbUser);
        user.Roles = userRoles;
        user.Addresses = _mapper.Map<IEnumerable<Address>>(addresses);

        return user;
    }

    public async Task<Result<User>> AddAddressAsync(SaveAddressRequest address)
    {
        var validationResult = await _addressValidator.ValidateAsync(address);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.ToValidationErrors();
            return Result.Fail(FailureReasons.ClientError, "validation errors", validationErrors);
        }

        try
        {
            var userId = _claimService.GetId();
            var dbUser = await _userManager.FindByIdAsync(userId.ToString());

            var dbAddress = _mapper.Map<Entities.Address>(address);
            await _userManager.AddAddressAsync(dbUser, dbAddress);

            var user = _mapper.Map<User>(dbUser);
            return user;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<Result<StreamFileContent>> GetPhotoAsync()
    {
        var userId = _claimService.GetId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (!string.IsNullOrWhiteSpace(user.Photo))
        {
            var stream = await _storageProvider.ReadAsync(user.Photo);
            var contentType = MimeUtility.GetMimeMapping(user.Photo);

            var content = new StreamFileContent(stream, contentType, user.Photo);
            return content;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No image found");
    }

    public async Task<Result> AddPhotoAsync(string fileName, Stream stream)
    {
        try
        {
            var userId = _claimService.GetId();
            var path = Path.Combine("users", $"{userId}_{fileName}");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            user.Photo = path;

            await _userManager.UpdateAsync(user);
            await _storageProvider.SaveAsync(path, stream);
            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
        catch (IOException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public Task<Guid> GetUserIdAsync()
    {
        var userId = _claimService.GetId();
        return Task.FromResult(userId);
    }

    public Task<string> GetUserNameAsync()
    {
        var userName = _claimService.GetUserName();
        return Task.FromResult(userName);
    }

    public Task<ClaimsIdentity> GetIdentityAsync()
    {
        var identity = _claimService.GetIdentity();
        return Task.FromResult(identity);
    }

    public Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var identity = _claimService.GetIdentity();
        return Task.FromResult(identity.Claims);
    }
}