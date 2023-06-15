using FluentValidation.Results;
using OperationResults;

namespace CommerceApi.BusinessLayer.RemoteServices;

public static class ValidationErrorService
{
    public static IEnumerable<ValidationError> GetErrors(ValidationResult validationResult)
    {
        var validationErrors = new List<ValidationError>();
        foreach (var error in validationResult.Errors)
        {
            validationErrors.Add(new(error.PropertyName, error.ErrorMessage));
        }

        return validationErrors;
    }
}