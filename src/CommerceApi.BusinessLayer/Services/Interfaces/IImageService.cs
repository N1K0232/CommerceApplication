using CommerceApi.Shared.Models;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;

public interface IImageService
{
    Task<Result> DeleteAsync(Guid imageId);

    Task<IEnumerable<Image>> GetListAsync();

    Task<Result<StreamFileContent>> GetAsync(Guid imageId);

    Task<Result> UploadAsync(Stream stream, string fileName, string title, string description);
}