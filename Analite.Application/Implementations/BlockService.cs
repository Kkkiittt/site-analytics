
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class BlockService : IBlockService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	private readonly ILogger _log;

	public BlockService(AppDbContext db, IIdentityService id,  ILogger<BlockService> log)
	{
		_db = db;
		_id = id;
		_log = log;
	}

	public async Task<BlockGetDto> CreateBlockAsync(BlockCreateDto dto)
	{
		_log.LogInformation("[{Time}] User {UserId} attempts to create a block on Page {PageId}",
			DateTime.UtcNow, 
			_id.Id, 
			dto.PageId);

		try
		{
			Page page = await _db.Pages
				            .FirstOrDefaultAsync(p => p.Id == dto.PageId)
			            ?? throw new NotFoundException("Page");
			if (page.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} tried to create block on another user's page {PageId}",
					_id.Id, dto.PageId);
				throw new NoAccessException("others' pages");
			}
				

			Block entity = new()
			{
				CustomerId = _id.Id,
				Description = dto.Description,
				Name = dto.Name,
				PageId = dto.PageId
			};
			_db.Blocks.Add(entity);
			await _db.SaveChangesAsync();
			
			_log.LogInformation("Block created successfully: BlockId {BlockId}, User {UserId}, PageId {PageId}",
				entity.Id, 
				_id.Id,
				dto.PageId);
			
			return new BlockGetDto
			{
				Id = entity.Id,
				Description = entity.Description,
				Name = entity.Name,
				PageId = entity.PageId,
				PageName = page.Name,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error creating block by User {UserId} on Page {PageId}", 
				_id.Id ,
				dto.PageId);
			throw;
		}
		
		
	}

	public async Task DeleteBlockAsync(long id)
	{
		_log.LogInformation(
			"🗑 [{Time}] User {UserId} attempts to delete Block {BlockId}",
			DateTime.UtcNow, _id.Id, id);

		try
		{
			Block entity = await _db.Blocks.FindAsync(id)
			               ?? throw new NotFoundException("Block");

			if (entity.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} tried to delete another user's block {BlockId}",
					_id.Id, 
					id);
				throw new NoAccessException("others' blocks");
			}
				
			_db.Blocks.Remove(entity);
			await _db.SaveChangesAsync();
			
			_log.LogInformation("Block {BlockId} successfully deleted by User {UserId}", 
				id,
				_id.Id);
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error deleting block {BlockId} by User {UserId}", 
				id,
				_id.Id);
			throw;
		}
	}
		

	public async Task<BlockGetDto> GetByIdAsync(long id)
	{
		_log.LogDebug("[{Time}] User {UserId} requests Block {BlockId}",
			DateTime.UtcNow, 
			_id.Id,
			id);

		try
		{
			Block entity = await _db.Blocks
				               .Include(b => b.Page)
				               .FirstOrDefaultAsync(b => b.Id == id)
			               ?? throw new NotFoundException("Block");

			if (entity.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} attempted to access another user's block {BlockId}",
					_id.Id, 
					id);
				throw new NoAccessException("others' blocks");
			}
			
			_log.LogInformation("Block {BlockId} successfully retrieved by User {UserId}", 
				id, 
				_id.Id);
				
			return new BlockGetDto()
			{
				Description = entity.Description,
				Id = entity.Id,
				Name = entity.Name,
				PageId = entity.PageId,
				PageName = entity.Page.Name,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error retrieving block {BlockId} for User {UserId}", id, _id.Id);
			throw;
		}
		
	}

	public async Task<IEnumerable<BlockGetDto>> GetByPageAsync(long pageId)
	{
		_log.LogDebug("[{Time}] User {UserId} requests blocks for Page {PageId}",
			DateTime.UtcNow,
			_id.Id,
			pageId);

		try
		{
			Page page = await _db.Pages.Include(p => p.Blocks)
				            .FirstOrDefaultAsync(p => p.Id == pageId)
			            ?? throw new NotFoundException("Page");

			if (page.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} tried to access another user's page {PageId}",
					_id.Id, 
					pageId);
				throw new NoAccessException("others' pages");
			}
			
			_log.LogInformation("Retrieved {Count} blocks for Page {PageId} by User {UserId}",
				page.Blocks.Count,
				pageId,
				_id.Id);
				
			return page.Blocks.Select(b => new BlockGetDto()
			{
				Description = b.Description,
				Id = b.Id,
				Name = b.Name,
				PageId = b.PageId,
				PageName = page.Name,
			});
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error fetching blocks for Page {PageId} by User {UserId}", pageId, _id.Id);
			throw;
		}
	}

	public async Task<BlockGetDto> UpdateBlockAsync(long id, BlockCreateDto dto)
	{
		_log.LogInformation(
			"[{Time}] User {UserId} attempts to update Block {BlockId}",
			DateTime.UtcNow, _id.Id, id);

		try
		{
			Block entity = await _db.Blocks.FindAsync(id)
			               ?? throw new NotFoundException("Block");
			
			if (entity.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} attempted to update another user's block {BlockId}",
					_id.Id, id);
				throw new NoAccessException("others' blocks");

			}
			entity.Name = dto.Name;
			entity.Description = dto.Description;
			
			if (dto.PageId != entity.PageId)
			{
				Page page = await _db.Pages
					            .FirstOrDefaultAsync(p => p.Id == dto.PageId)
				            ?? throw new NotFoundException("Page");

				if (page.CustomerId != _id.Id)
				{
					_log.LogWarning("User {UserId} attempted to move block {BlockId} to another user's page {PageId}",
						_id.Id, 
						id, 
						dto.PageId);
					throw new NoAccessException("others' pages");

				}
				entity.PageId = dto.PageId;
			}

			await _db.SaveChangesAsync();
			
			Page updatedPage = await _db.Pages.FirstAsync(p => p.Id == entity.PageId);
			
			_log.LogInformation("Block {BlockId} updated by User {UserId}", id, _id.Id);
			
			return new BlockGetDto
			{
				Id = entity.Id,
				Description = entity.Description,
				Name = entity.Name,
				PageId = entity.PageId,
				PageName = updatedPage.Name,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error updating Block {BlockId} by User {UserId}",
				id, 
				_id.Id);
			throw;
		}
		
		
	}
}