using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.Security.Abstractions;
using CommerceApi.Shared.Models;
using CommerceApi.StorageProviders.Abstractions;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = CommerceApi.DataAccessLayer.Entities;

namespace CommerceApi.BusinessLayer.Services;

public class ImageService : IImageService
{
    private readonly IDataContext _dataContext;
    private readonly IStorageProvider _storageProvider;
    private readonly IPathGenerator _pathGenerator;
    private readonly IMapper _mapper;

    public ImageService(IDataContext dataContext, IStorageProvider storageProvider, IPathGenerator pathGenerator, IMapper mapper)
    {
        _dataContext = dataContext;
        _storageProvider = storageProvider;
        _pathGenerator = pathGenerator;
        _mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid id");
        }

        try
        {
            var image = await _dataContext.GetAsync<Entities.Image>(imageId);
            if (image == null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, $"No image found with id {imageId}");
            }

            _dataContext.Delete(image);
            await _dataContext.SaveAsync();
            await _storageProvider.DeleteAsync(image.DownloadFileName);

            return Result.Ok();
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

    public async Task<IEnumerable<Image>> GetListAsync()
    {
        var query = _dataContext.Get<Entities.Image>();
        var images = await query.OrderBy(i => i.DownloadFileName)
            .ProjectTo<Image>(_mapper.ConfigurationProvider)
            .ToListAsync();

        foreach (var image in images)
        {
            image.ContentType ??= MimeUtility.GetMimeMapping(image.DownloadFileName);
        }

        return images;
    }

    public async Task<Result<StreamFileContent>> GetAsync(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            return Result.Fail(FailureReasons.ClientError, "invalid id");
        }

        var image = await _dataContext.GetAsync<Entities.Image>(imageId);
        if (image is not null)
        {
            var stream = await _storageProvider.ReadAsync(image.DownloadFileName);
            var contentType = image.ContentType ?? MimeUtility.GetMimeMapping(image.DownloadFileName);

            var content = new StreamFileContent(stream, contentType, image.DownloadFileName);
            return content;
        }

        return Result.Fail(FailureReasons.ItemNotFound, "no image found");
    }

    public async Task<Result<Image>> UploadAsync(StreamFileContent content, string title, string description)
    {
        try
        {
            var extension = Path.GetExtension(content.DownloadFileName)[1..];
            var contentType = content.ContentType ?? MimeUtility.GetMimeMapping(content.DownloadFileName);

            var image = new Entities.Image
            {
                DownloadFileName = content.DownloadFileName,
                ContentType = contentType,
                Extension = extension,
                Length = content.Content.Length,
                Title = title,
                Description = description
            };

            _dataContext.Create(image);
            await _dataContext.SaveAsync();
            await _storageProvider.SaveAsync(content.DownloadFileName, content.Content);

            var uploadedImage = _mapper.Map<Image>(image);
            return uploadedImage;
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
}