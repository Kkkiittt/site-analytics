using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IBlockService
{
    Task<BlockGetDto> CreateBlockAsync(BlockCreateDto blockCreateDto);
    Task<BlockGetDto> UpdateBlockAsync(long id , BlockCreateDto blockCreateDto);
    Task<BlockGetDto> DeleteBlockAsync(long id);

    Task<BlockGetDto?> GetByIdAsync(long id);
    Task<IEnumerable<BlockGetDto>> GetAllAsync(long pageId);

    Task<int> GetHoversCountAsync(long blockId);
    Task<int> GetClicksCountAsync(long blockId);
}