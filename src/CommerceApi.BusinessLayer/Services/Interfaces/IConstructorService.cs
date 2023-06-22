using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using OperationResults;

namespace CommerceApi.BusinessLayer.Services.Interfaces;
public interface IConstructorService
{
    Task<Result<Constructor>> CreateAsync(SaveConstructorRequest constructor);
    Task<Result> DeleteAsync(Guid constructorId);
    Task<Result<Constructor>> GetAsync(Guid constructorId);
    Task<IEnumerable<Constructor>> GetListAsync(string city);
    Task<Result<Constructor>> UpdateAsync(Guid constructorId, SaveConstructorRequest constructor);
}