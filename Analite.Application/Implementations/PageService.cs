
using Analite.Application.Dtos;
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class PageService : IPageService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	private readonly ILogger<PageService> _log;

	public PageService(IIdentityService id, AppDbContext db, ILogger<PageService> log)
	{
		_id = id;
		_db = db;
		_log = log;
	}

	public async Task<PageGetDto> CreatePageAsync(PageCreateDto dto)
	{
		_log.LogInformation("User {UserId} attempts to create a page for CustomerId {CustomerId}",
			_id.Id, 
			dto.CustomerId);

		if (dto.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} tried to create a page for another customer {CustomerId}",
				_id.Id,
				dto.CustomerId);
			throw new NoAccessException("other users' pages");
		}
			
		Page entity = new Page()
		{
			CustomerId = _id.Id,
			Description = dto.Description,
			Name = dto.Name,
			Order = dto.Order
		};
		_db.Pages.Add(entity);
		await _db.SaveChangesAsync();
		
		_log.LogInformation(
			"Page {PageId} created by User {UserId}",
			entity.Id, _id.Id);
		
		return new PageGetDto()
		{
			Description = entity.Description,
			Id = entity.Id,
			Name = entity.Name,
			Blocks = []
		};
	}

	public async Task DeletePageAsync(long id)
	{
		_log.LogInformation("User {UserId} attempts to delete Page {PageId}", 
			_id.Id,
			id);
		
		Page entity = await _db.Pages.FindAsync(id) ?? throw new NotFoundException("Page");
		if (entity.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} tried to delete another user's page {PageId}",
				_id.Id,
				id);
			throw new NoAccessException("Others' pages");
		}
			
		_db.Remove(entity);
		await _db.SaveChangesAsync();
		
		_log.LogInformation("Page {PageId} deleted by User {UserId}", 
			id,
			_id.Id);
	}

	public async Task<IEnumerable<PageGetDto>> GetByCustomerAsync(Guid? id)
	{
		id ??= _id.Id;
		if (id != _id.Id)
		{
			_log.LogWarning("User {UserId} attempted to fetch pages of another customer {TargetId}",
				_id.Id, id);
			throw new NoAccessException("Others' pages");
		}
		_log.LogDebug("User {UserId} retrieving all pages for CustomerId {CustomerId}",
			_id.Id, 
			id);
			
		var pages = await _db.Pages
					.Where(p => p.CustomerId == id)
					.Select(p => new PageGetDto()
					{
						Blocks = p.Blocks.Select(b => new ShortDto()
						{
							Id = b.Id.ToString(),
							Name = b.Name
						}).ToList(),
						Description = p.Description,
						Id = p.Id,
						Name = p.Name,
						Order = p.Order,
					}).ToListAsync();
		_log.LogInformation("Retrieved {Count} pages for CustomerId {CustomerId}",
			pages.Count,
			id);
		return pages;
	}

	public async Task<PageGetDto> GetByIdAsync(long id)
	{
		_log.LogDebug("User {UserId} requests Page {PageId}", 
			_id.Id,
			id);
		
		Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Page");
		if (entity.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} attempted to access another user's page {PageId}", _id.Id, id);
			throw new NoAccessException("Others' pages");
		}
		_log.LogInformation("Page {PageId} retrieved by User {UserId}",
			id,
			_id.Id);
		
		return new PageGetDto()
		{
			Blocks = entity.Blocks.Select(b => new ShortDto()
			{
				Id = b.Id.ToString(),
				Name = b.Name
			}).ToList(),
			Order = entity.Order,
			Description = entity.Description,
			Id = entity.Id,
			Name = entity.Name,
		};
	}

	public async Task<int> GetUniqueUsersCountsAsync(long pageId)
	{
		_log.LogDebug("Counting unique users for Page {PageId} requested by User {UserId}", 
			pageId, 
			_id.Id);
		
		Page entity = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");
		if (entity.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} attempted to access unique user count for other's Page {PageId}",
				_id.Id,
				pageId);
			throw new NoAccessException("Others' pages");
		}
		int count = await _db.Events.Where(e => e.PageId == pageId).Select(e => e.SessionId).Distinct()
			.CountAsync();
		_log.LogInformation("Unique users for Page {PageId}: {Count}",
			pageId,
			count);
		return count;	}

	public async Task<int> GetVisitsCountsAsync(long pageId)
	{
		_log.LogDebug("Counting visits for Page {PageId} requested by User {UserId}", 
			pageId, 
			_id.Id);
		
		Page entity = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");
		if (entity.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} attempted to access visits count for other's Page {PageId}",
				_id.Id,
				pageId);
			throw new NoAccessException("Others' pages");
		}
		int count = await _db.Events.Where(e => e.PageId == pageId).Select(e => e.SessionId).CountAsync();
		_log.LogInformation("Visits count for Page {PageId}: {Count}",
			pageId,
			count);
		return count;	}

	public async Task<PageGetDto> UpdatePageAsync(long id, PageCreateDto dto)
	{
		_log.LogInformation("User {UserId} attempts to update Page {PageId}", 
			_id.Id, 
			id);
		
		Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Page");
		if (entity.CustomerId != _id.Id)
		{
			_log.LogWarning("User {UserId} attempted to update another user's page {PageId}",
				_id.Id,
				id);
			throw new NoAccessException("Others' pages");
		}
		entity.Name = dto.Name;
		entity.Description = dto.Description;
		entity.Order = dto.Order;
		await _db.SaveChangesAsync();
		_log.LogInformation("Page {PageId} updated by User {UserId}",
			id,
			_id.Id);
		
		return new PageGetDto()
		{
			Blocks = entity.Blocks.Select(b => new ShortDto()
			{
				Id = b.Id.ToString(),
				Name = b.Name,
			}).ToList(),
			Order = entity.Order,
			Description = entity.Description,
			Id = entity.Id,
			Name = entity.Name,
		};
	}
}