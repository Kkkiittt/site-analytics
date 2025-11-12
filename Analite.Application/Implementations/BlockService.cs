
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class BlockService : IBlockService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;

	public BlockService(AppDbContext db, IIdentityService id)
	{
		_db = db;
		_id = id;
	}

	public async Task<BlockGetDto> CreateBlockAsync(BlockCreateDto dto)
	{
		Page page = await _db.Pages
			.FirstOrDefaultAsync(p => p.Id == dto.PageId)
			?? throw new NotFoundException("Page");
		if(page.CustomerId != _id.Id)
			throw new NoAccessException("others' pages");

		Block entity = new()
		{
			CustomerId = _id.Id,
			Description = dto.Description,
			Name = dto.Name,
			PageId = dto.PageId
		};
		_db.Blocks.Add(entity);
		await _db.SaveChangesAsync();
		return new BlockGetDto
		{
			Id = entity.Id,
			Description = entity.Description,
			Name = entity.Name,
			PageId = entity.PageId,
			PageName = page.Name,
		};
	}

	public async Task DeleteBlockAsync(long id)
	{
		Block entity = await _db.Blocks.FindAsync(id)
			?? throw new NotFoundException("Block");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("others' blocks");
		_db.Blocks.Remove(entity);
		await _db.SaveChangesAsync();
	}

	public async Task<BlockGetDto> GetByIdAsync(long id)
	{
		Block entity = await _db.Blocks
			.Include(b => b.Page)
			.FirstOrDefaultAsync(b => b.Id == id)
			?? throw new NotFoundException("Block");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("others' blocks");
		return new BlockGetDto()
		{
			Description = entity.Description,
			Id = entity.Id,
			Name = entity.Name,
			PageId = entity.PageId,
			PageName = entity.Page.Name,
		};
	}

	public async Task<IEnumerable<BlockGetDto>> GetByPageAsync(long pageId)
	{
		Page page = await _db.Pages.Include(p => p.Blocks)
			.FirstOrDefaultAsync(p => p.Id == pageId)
			?? throw new NotFoundException("Page");
		if(page.CustomerId != _id.Id)
			throw new NoAccessException("others' pages");
		return page.Blocks.Select(b => new BlockGetDto()
		{
			Description = b.Description,
			Id = b.Id,
			Name = b.Name,
			PageId = b.PageId,
			PageName = page.Name,
		});
	}

	public async Task<BlockGetDto> UpdateBlockAsync(long id, BlockCreateDto dto)
	{
		Block entity = await _db.Blocks.FindAsync(id)
			?? throw new NotFoundException("Block");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("others' blocks");
		entity.Name = dto.Name;
		entity.Description = dto.Description;
		if(dto.PageId!= entity.PageId)
		{
			Page page = await _db.Pages
				.FirstOrDefaultAsync(p => p.Id == dto.PageId)
				?? throw new NotFoundException("Page");
			if(page.CustomerId != _id.Id)
				throw new NoAccessException("others' pages");
			entity.PageId = dto.PageId;
		}
		await _db.SaveChangesAsync();
		Page updatedPage = await _db.Pages.FirstAsync(p => p.Id == entity.PageId);
		return new BlockGetDto
		{
			Id = entity.Id,
			Description = entity.Description,
			Name = entity.Name,
			PageId = entity.PageId,
			PageName = updatedPage.Name,
		};
	}
}