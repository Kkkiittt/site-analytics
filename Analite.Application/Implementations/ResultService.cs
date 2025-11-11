
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

	public ResultService(AppDbContext db)
	{
		_db = db;
	}

	public async Task<ConversionDto> GetConversionAsync(Guid customerId, DateTime? from, DateTime? to)
	{
		var query = _db.Events.Where(e => e.CustomerId == customerId);
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
			.Join(_db.Pages, s => s.Key, p => p.Id, (s, p) => new ClickDto()
			{
				Clicks = s.Clicks,
				Id = p.Id.ToString(),
				Name = p.Name,
			})
			.ToListAsync();
		return new ConversionDto()
		{
			From = from ?? DateTime.MinValue,
			To = to ?? DateTime.MaxValue,
			Pages = res
		};
	}

	public async Task<HeatmapDto> GetHeatmapAsync(long pageId, DateTime? from, DateTime? to)
	{

		var page = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");

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

		var res = await query.GroupBy(e => e.BlockId)
			.Select(g => new { g.Key, Clicks = g.Count() })
			.Join(_db.Blocks, s => s.Key, b => b.Id, (s, b) => new ClickDto()
			{
				Clicks = s.Clicks,
				Id = b.Id.ToString(),
				Name = b.Name,
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