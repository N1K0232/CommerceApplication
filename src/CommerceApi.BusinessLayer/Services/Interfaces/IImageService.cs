using CommerceApi.BusinessLayer.Models;
using CommerceApi.Shared.Models;
using OperationResults;
using StreamFileContent = CommerceApi.BusinessLayer.Models.StreamFileContent;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IImageService : IDisposable
{
    Task<Result> DeleteAsync(Guid imageId);

    Task<IEnumerable<Image>> GetAsync();

    Task<ImageStream> GetAsync(Guid imageId);

    Task<Result> UploadAsync(StreamFileContent content);
}