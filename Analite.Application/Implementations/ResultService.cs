using Analite.Application.Dtos;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class ResultService : IResultService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;

	public ResultService(AppDbContext db, IIdentityService id)
	{
		_db = db;
		_id = id;
	}

	public async Task<ConversionDto> GetConversionAsync(Guid? id, DateTime? from, DateTime? to)
	{
		id ??= _id.Id;
		if(id != _id.Id)
			throw new NoAccessException("Others' conversion");
		var query = _db.Events.Where(e => e.CustomerId == id);
		if(from != null)
		{
			query = query.Where(e => e.OccuredAt >= from);
		}
		if(to != null)
		{
			query = query.Where(e => e.OccuredAt <= to);
		}
		var res = await query.GroupBy(e => e.PageId)
			.Select(g => new { g.Key, Clicks = g.Count() })
			.Join(_db.Pages, s => s.Key, p => p.Id, (s, p) => new
			{
				Clicks = s.Clicks,
				Id = p.Id.ToString(),
				Name = p.Name,
				Order = p.Order,
			})
			.OrderBy(e => e.Order)
			.ToListAsync();
		float total = res.FirstOrDefault()?.Clicks ?? 0;
		return new ConversionDto()
		{
			From = from ?? DateTime.MinValue,
			To = to ?? DateTime.MaxValue,
			Pages = res.Select(r => new ClickDto()
			{
				Clicks = r.Clicks,
				Id = r.Id,
				Name = r.Name,
				Percentage = r.Clicks / total
			})
		};
	}

	public async Task<HeatmapDto> GetHeatmapAsync(long pageId, DateTime? from, DateTime? to)
	{

		var page = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");

		if(page.CustomerId != _id.Id)
			throw new NoAccessException("Others' pages");

		var customerId = page.CustomerId;

		var query = _db.Events.Where(e => e.CustomerId == customerId)
			.Where(e => e.PageId == pageId);

		if(from != null)
		{
			query = query.Where(e => e.OccuredAt >= from);
		}

		if(to != null)
		{
			query = query.Where(e => e.OccuredAt <= to);
		}

		long total = await query.CountAsync();
		var res = await query.GroupBy(e => e.BlockId)
			.Select(g => new { g.Key, Clicks = g.Count() })
			.Join(_db.Blocks, s => s.Key, b => b.Id, (s, b) => new ClickDto()
			{
				Clicks = s.Clicks,
				Id = b.Id.ToString(),
				Name = b.Name,
				Percentage = (float)s.Clicks / total
			})
			.ToListAsync();

		return new HeatmapDto()
		{
			From = from ?? DateTime.MinValue,
			To = to ?? DateTime.MaxValue,
			Page = new ShortDto()
			{
				Id = page.Id.ToString(),
				Name = page.Name,
			},
			Blocks = res
		};
	}
}