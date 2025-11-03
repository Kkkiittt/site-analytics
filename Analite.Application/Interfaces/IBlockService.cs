using Analite.Application.Dtos.Create;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IBlockService
{
    Task<Block> CreateBlockAsync(BlockCreateDto blockCreateDto);
    Task<Block> UpdateBlockAsync(long id , BlockCreateDto blockCreateDto);
    Task<Block> DeleteBlockAsync(long id);

    Task<Block?> GetByIdAsync(long id);
    Task<IEnumerable<Block>> GetAllAsync(long pageId);

    Task<int> GetHoversCountAsync(long blockId);
    Task<int> GetClicksCountAsync(long blockId);
}