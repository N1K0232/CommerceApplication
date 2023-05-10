using CommerceApi.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommerceApi.Requests;

public class UploadImageRequest
{
    [AllowedExtensions(".jpg", "*.jpeg", "*.png")]
    [BindRequired]
    public IFormFile File { get; set; }

    [BindRequired]
    public string Title { get; set; }

    public string Description { get; set; }
}