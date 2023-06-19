using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace CommerceApi.BusinessLayer.Extensions;

public static class ValidatioResultExtensions
{
    public static IEnumerable<ValidationError> ToValidationErrors(this ValidationResult validationResult)
    {
        var validationErrors = new List<ValidationError>(validationResult.Errors.Count);
        foreach (var error in validationResult.Errors)
        {
            validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
        }

        return validationErrors;
    }

    public static IEnumerable<ValidationError> ToValidationErrors(this IdentityResult identityResult)
    {
        var validationErrors = new List<ValidationError>(identityResult.Errors.Count());
        foreach (var error in identityResult.Errors)
        {
            validationErrors.Add(new(error.Code, error.Description));
        }

        return validationErrors;
    }
}