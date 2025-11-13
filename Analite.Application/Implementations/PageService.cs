
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
		_log.LogInformation("[{Time}] User {UserId} attempts to create a page for CustomerId {CustomerId}",
			DateTime.UtcNow, 
			_id.Id, 
			dto.CustomerId);

		try
		{
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
			};
			_db.Pages.Add(entity);
			await _db.SaveChangesAsync();

			_log.LogInformation("Page {PageId} created by User {UserId}",
				entity.Id,
				_id.Id);

			return new PageGetDto()
			{
				Description = entity.Description,
				Id = entity.Id,
				Name = entity.Name,
				Blocks = []
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error creating page for customer {CustomerId} by User {UserId}", 
				dto.CustomerId, 
				_id.Id);
			throw;
		}
	}

	public async Task DeletePageAsync(long id)
	{
		_log.LogInformation("[{Time}] User {UserId} attempts to delete Page {PageId}", 
			DateTime.UtcNow,
			_id.Id,
			id);

		try
		{
			Page entity = await _db.Pages.FindAsync(id) ?? throw new NotFoundException("Page");

			if (entity.CustomerId != _id.Id)
			{
				_log.LogWarning("⚠️ User {UserId} tried to delete another user's page {PageId}", _id.Id, id);
				throw new NoAccessException("Others' pages");
			}
			_db.Remove(entity);
			await _db.SaveChangesAsync();
			
			_log.LogInformation("Page {PageId} deleted by User {UserId}", 
				id,
				_id.Id);

		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error deleting Page {PageId} by User {UserId}", 
				id, 
				_id.Id);
			throw;
		}

	}

	public async Task<IEnumerable<PageGetDto>> GetByCustomerAsync(Guid customerId)
	{
		_log.LogDebug("[{Time}] Fetching pages for customer {CustomerId} requested by User {UserId}", DateTime.UtcNow, customerId, _id.Id);

		try
		{
			var pages = await _db.Pages
				.Where(p => p.CustomerId == customerId)
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
				}).ToListAsync();

			_log.LogInformation("Retrieved {Count} pages for CustomerId {CustomerId} by User {UserId}",
				pages.Count,
				customerId,
				_id.Id);
			return pages;

		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error fetching pages for CustomerId {CustomerId} by User {UserId}", 
				customerId, 
				_id.Id);
			throw;
		}
	}

	public async Task<PageGetDto> GetByIdAsync(long id)
	{
		_log.LogDebug("[{Time}] User {UserId} requests Page {PageId}", 
			DateTime.UtcNow, 
			_id.Id,
			id);

		try
		{
			Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ??
			              throw new NotFoundException("Page");
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
				Description = entity.Description,
				Id = entity.Id,
				Name = entity.Name,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error retrieving Page {PageId} by User {UserId}", 
				id, 
				_id.Id);
			throw;
		}
		
	}

	public async Task<int> GetUniqueUsersCountsAsync(long pageId)
	{
		_log.LogDebug("[{Time}] Counting unique users for Page {PageId} requested by User {UserId}", 
			DateTime.UtcNow, 
			pageId, 
			_id.Id);


		try
		{
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
			return count;
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error counting unique users for Page {PageId} by User {UserId}", 
				pageId, 
				_id.Id);
			throw;
		}
		
	}

	public async Task<int> GetVisitsCountsAsync(long pageId)
	{
		_log.LogDebug("[{Time}] Counting visits for Page {PageId} requested by User {UserId}", 
			DateTime.UtcNow, 
			pageId, 
			_id.Id);

		try
		{
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
			return count;
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error counting visits for Page {PageId} by User {UserId}",
				pageId, 
				_id.Id);
			throw;
		}
	}

	public async Task<PageGetDto> UpdatePageAsync(long id, PageCreateDto dto)
	{
		_log.LogInformation("[{Time}] User {UserId} attempts to update Page {PageId}", 
			DateTime.UtcNow,
			_id.Id, 
			id);

		try
		{
			Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ??
			              throw new NotFoundException("Page");

			if (entity.CustomerId != _id.Id)
			{
				_log.LogWarning("User {UserId} attempted to update another user's page {PageId}",
					_id.Id,
					id);
				throw new NoAccessException("Others' pages");
			}

			entity.Name = dto.Name;
			entity.Description = dto.Description;
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
				Description = entity.Description,
				Id = entity.Id,
				Name = entity.Name,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error updating Page {PageId} by User {UserId}", 
				id,
				_id.Id);
			throw;
		}
		
	}
}