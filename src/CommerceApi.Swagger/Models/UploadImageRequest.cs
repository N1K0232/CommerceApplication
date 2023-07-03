using CommerceApi.Swagger.Filters;
using Microsoft.AspNetCore.Http;

namespace CommerceApi.Swagger.Models;

public class UploadImageRequest
{
    [AllowedExtensions("*.jpg", "*jpeg", "*.png")]
    public IFormFile File { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }
}