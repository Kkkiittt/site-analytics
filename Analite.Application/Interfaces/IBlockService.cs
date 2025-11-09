using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;

namespace Analite.Application.Interfaces;

public interface IBlockService
{
	Task<BlockGetDto> CreateBlockAsync(BlockCreateDto blockCreateDto);
	Task<BlockGetDto> UpdateBlockAsync(long id, BlockCreateDto blockCreateDto);
	Task DeleteBlockAsync(long id);

	Task<BlockGetDto?> GetByIdAsync(long id);
	Task<IEnumerable<BlockGetDto>> GetByPageAsync(long pageId);
}