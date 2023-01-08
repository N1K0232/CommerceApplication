using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Models;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer;
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

        var image = await dataContext.GetAsync<Entities.Image>(imageId);
        if (image is not null)
        {
            dataContext.Delete(image);
            await dataContext.SaveAsync();
            await storageProvider.DeleteAsync(image.Path);

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, "No image found");
    }

    public async Task<IEnumerable<Image>> GetAsync()
    {
        var images = await dataContext.GetData<Entities.Image>()
            .OrderBy(i => i.Path)
            .ProjectTo<Image>(mapper.ConfigurationProvider)
            .ToListAsync();

        return images;
    }

    public async Task<ImageStream> GetAsync(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            return null;
        }

        var image = await dataContext.GetAsync<Entities.Image>(imageId);
        if (image is not null)
        {
            var stream = await storageProvider.ReadAsync(image.Path);
            var contentType = MimeUtility.GetMimeMapping(image.Path);

            var imageStream = new ImageStream(stream, contentType);
            return imageStream;
        }

        return null;
    }

    public async Task<Result> UploadAsync(StreamFileContent content)
    {
        var path = CreatePath(content.FileName);

        var image = await dataContext.GetData<Entities.Image>(trackingChanges: true).FirstOrDefaultAsync(i => i.FileName.Contains(content.FileName));
        if (image is null)
        {
            image = new Entities.Image
            {
                FileName = content.FileName,
                Path = path,
                ContentType = content.ContentType,
                Length = content.Length,
                Description = content.Description
            };

            dataContext.Create(image);
        }
        else
        {
            image.Description = content.Description;
            dataContext.Edit(image);
        }

        try
        {
            await dataContext.SaveAsync();
            await storageProvider.UploadAsync(path, content.Stream);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(FailureReasons.DatabaseError, ex);
        }
    }

    private static string CreatePath(string fileName)
    {
        var now = DateTime.UtcNow;
        return Path.Combine(now.Year.ToString(), now.Month.ToString("00"), fileName);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            dataContext.Dispose();
            dataContext = null;

            storageProvider.Dispose();
            storageProvider = null;
        }
    }
}