﻿using System.ComponentModel.DataAnnotations;

namespace CommerceApi.Filters;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly IEnumerable<string> extensions;

    public AllowedExtensionsAttribute(params string[] extensions)
    {
        this.extensions = extensions.Select(e => e.ToLowerInvariant().Replace("*.", string.Empty));
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant()[1..];
            if (!extensions.Contains(extension))
            {
                return new ValidationResult(GetErrorMessage());
            }
        }

        return ValidationResult.Success;
    }

    private string GetErrorMessage()
    {
        return $"the system only supports these extensions: {string.Join(",", extensions)}";
    }
}