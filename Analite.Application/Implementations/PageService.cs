
using Analite.Application.Dtos;
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class PageService : IPageService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;

	public PageService(IIdentityService id, AppDbContext db)
	{
		_id = id;
		_db = db;
	}

	public async Task<PageGetDto> CreatePageAsync(PageCreateDto dto)
	{
		if(dto.CustomerId != _id.Id)
			throw new NoAccessException("other users' pages");
		Page entity = new Page()
		{
			CustomerId = _id.Id,
			Description = dto.Description,
			Name = dto.Name,
		};
		_db.Pages.Add(entity);
		await _db.SaveChangesAsync();
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
		Page entity = await _db.Pages.FindAsync(id) ?? throw new NotFoundException("Page");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");
		_db.Remove(entity);
		await _db.SaveChangesAsync();
	}

	public async Task<IEnumerable<PageGetDto>> GetByCustomerAsync(Guid customerId)
	{
		return await _db.Pages
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
	}

	public async Task<PageGetDto> GetByIdAsync(long id)
	{
		Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Page");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");
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

	public async Task<int> GetUniqueUsersCountsAsync(long pageId)
	{
		Page entity = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");
		return await _db.Events.Where(e => e.PageId == pageId).Select(e => e.SessionId).Distinct().CountAsync();
	}

	public async Task<int> GetVisitsCountsAsync(long pageId)
	{
		Page entity = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");
		return await _db.Events.Where(e => e.PageId == pageId).Select(e => e.SessionId).CountAsync();
	}

	public async Task<PageGetDto> UpdatePageAsync(long id, PageCreateDto dto)
	{
		Page entity = await _db.Pages.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Page");
		if(entity.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");
		entity.Name = dto.Name;
		entity.Description = dto.Description;
		await _db.SaveChangesAsync();
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
}