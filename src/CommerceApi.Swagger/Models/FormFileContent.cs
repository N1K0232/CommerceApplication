using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommerceApi.Swagger.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommerceApi.Swagger.Models;

public class FormFileContent
{
    [BindRequired]
    [AllowedExtensions("*.jpg", "*.jpeg", "*.png")]
    public IFormFile File { get; set; } = default!;

    [Required]
    [NotNull]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}