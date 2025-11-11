using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;

namespace Analite.Application.Interfaces;

public interface IPageService
{
	Task<PageGetDto> CreatePageAsync(PageCreateDto pageCreateDto);
	Task<PageGetDto> UpdatePageAsync(long id, PageCreateDto pageCreateDto);
	Task DeletePageAsync(long id);

	Task<PageGetDto> GetByIdAsync(long id);
	Task<IEnumerable<PageGetDto>> GetByCustomerAsync(Guid customerId);

	Task<int> GetVisitsCountsAsync(long pageId);
	Task<int> GetUniqueUsersCountsAsync(long pageId);
}