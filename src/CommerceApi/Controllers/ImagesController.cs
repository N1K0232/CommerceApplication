using CommerceApi.Authentication.Common;
using CommerceApi.Authorization.Filters;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Extensions;
using CommerceApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class ImagesController : ControllerBase
{
    private readonly IImageService imageService;

    public ImagesController(IImageService imageService)
    {
        this.imageService = imageService;
    }


    [HttpDelete]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Delete(Guid imageId)
    {
        var result = await imageService.DeleteAsync(imageId);
        return CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> GetList()
    {
        var images = await imageService.GetAsync();
        return Ok(images);
    }

    [HttpGet("{imageId:guid}")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User)]
    public async Task<IActionResult> Get(Guid imageId)
    {
        var result = await imageService.GetAsync(imageId);
        return result.Success ?
            File(result.Content.Stream, result.Content.ContentType) :
            CreateResponse(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
    public async Task<IActionResult> Upload([FromForm] UploadImageRequest request)
    {
        var content = request.ToStreamFileContent();
        var result = await imageService.UploadAsync(content);

        return CreateResponse(result, StatusCodes.Status200OK);
    }
}