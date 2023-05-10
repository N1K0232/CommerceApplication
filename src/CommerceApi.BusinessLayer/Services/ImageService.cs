using AutoMapper;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.StorageProviders;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;
using StreamFileContent = CommerceApi.BusinessLayer.Models.StreamFileContent;

namespace CommerceApi.BusinessLayer.Services;

public class ImageService : IImageService
{
    private IDataContext dataContext;
    private IStorageProvider storageProvider;

    private readonly IMapper mapper;


    public ImageService(IDataContext dataContext, IStorageProvider storageProvider, IMapper mapper)
    {
        this.dataContext = dataContext;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
    }


    public async Task<Result> DeleteAsync(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.GenericError, "Invalid id");
        }

        try
        {
            var image = await dataContext.GetAsync<Entities.Image>(imageId);
            dataContext.Delete(image);

            await dataContext.SaveAsync();
            await storageProvider.DeleteAsync(image.Path);

            return Result.Ok();
        }
        catch (ArgumentNullException ex)
        {
            return Result.Fail(FailureReasons.ItemNotFound, ex);
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    public async Task<IEnumerable<Image>> GetListAsync()
    {
        var dbImages = await dataContext.GetData<Entities.Image>()
            .OrderBy(i => i.Path)
            .ToListAsync();

        var images = mapper.Map<IEnumerable<Image>>(dbImages);
        foreach (var image in images)
        {
            image.ContentType ??= MimeUtility.GetMimeMapping(image.Path);
        }

        return images;
    }

    public async Task<Result<ImageStream>> GetAsync(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var image = await dataContext.GetAsync<Entities.Image>(imageId);
        if (image is not null)
        {
            var stream = await storageProvider.ReadAsync(image.DownloadFileName);
            var contentType = MimeUtility.GetMimeMapping(image.Path);

            var imageStream = new ImageStream { Stream = stream, ContentType = contentType };
            return imageStream;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no image found");
    }

    public async Task<Result<Image>> UploadAsync(StreamFileContent content)
    {
        try
        {
            var path = CreatePath(content.FileName);

            var dbImage = new Entities.Image
            {
                FileName = content.FileName,
                Path = path,
                Title = content.Title,
                Length = content.Length,
                ContentType = content.ContentType,
                Description = content.Description
            };

            dataContext.Create(dbImage);

            await dataContext.SaveAsync();
            await storageProvider.UploadAsync(dbImage.DownloadFileName, content.Stream);

            var savedImage = mapper.Map<Image>(dbImage);
            return savedImage;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(FailureReasons.GenericError, ex);
        }
    }

    private static string CreatePath(string fileName)
    {
        var now = DateTime.UtcNow;
        return Path.Combine(now.Year.ToString("0000"), now.Month.ToString("00"), fileName);
    }
}