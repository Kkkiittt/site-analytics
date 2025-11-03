using Analite.Application.Dtos.Create;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IPageService
{
    Task<Page> CreatePageAsync(PageCreateDto pageCreateDto);
    Task<Page> UpdatePageAsync(long id , PageCreateDto pageCreateDto);
    Task<Page> DeletePageAsync(long id);
    
    Task<Page?> GetByIdAsync(long id);
    Task<IEnumerable<Page>> GetAllAsync(Guid customerId);
    
    Task<int> GetVisitsCountsAsync(long pageId);
    Task<int> GetUniqueUsersCountsAsync(long pageId);
}