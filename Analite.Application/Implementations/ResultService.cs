
using Analite.Application.Dtos;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class ResultService : IResultService
{
	private readonly AppDbContext _db;
	private readonly ILogger<ResultService> _log;

	public ResultService(AppDbContext db,  ILogger<ResultService> log)
	{
		_db = db;
		_log = log;
	}

	public async Task<ConversionDto> GetConversionAsync(Guid customerId, DateTime? from, DateTime? to)
	{
		_log.LogInformation("[{Time}] User {CustomerId} requested conversion analytics. From={From}, To={To}",
			DateTime.UtcNow,
			customerId, 
			from,
			to);

		try
		{
			var query = _db.Events.Where(e => e.CustomerId == customerId);
			if (from != null)
			{
				query = query.Where(e => e.OccuredAt >= from);
				_log.LogDebug("Filtering conversion by start date >= {From}", from);
			}

			if (to != null)
			{
				query = query.Where(e => e.OccuredAt <= to);
				_log.LogDebug("Filtering conversion by end date <= {To}", to);
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
			
			_log.LogInformation("Conversion analytics generated for User {CustomerId}. Pages={Count}",
				customerId, 
				res.Count);
			
			return new ConversionDto()
			{
				From = from ?? DateTime.MinValue,
				To = to ?? DateTime.MaxValue,
				Pages = res
			};
		}
		catch (Exception ex)
		{
			_log.LogError(
				ex, "Error while generating conversion analytics for User {CustomerId}",
				customerId);
			throw;
		}
		
	}

	public async Task<HeatmapDto> GetHeatmapAsync(long pageId, DateTime? from, DateTime? to)
	{
		_log.LogInformation("[{Time}] Heatmap requested for Page {PageId}. From={From}, To={To}",
			DateTime.UtcNow, 
			pageId, 
			from,
			to);

		try
		{
			var page = await _db.Pages.FindAsync(pageId) ?? throw new NotFoundException("Page");

			var customerId = page.CustomerId;

			_log.LogDebug("Found page {PageId} belonging to Customer {CustomerId}",
				pageId,
				customerId);
			
			var query = _db.Events.Where(e => e.CustomerId == customerId)
				.Where(e => e.PageId == pageId);

			if (from != null)
			{
				query = query.Where(e => e.OccuredAt >= from);
			}

			if (to != null)
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
			
			_log.LogInformation("Heatmap generated for Page {PageId}. Blocks={Count}",
				pageId, 
				res.Count);
			
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
		catch (Exception ex)
		{
			_log.LogError(ex, "Error while generating heatmap for Page {PageId}", pageId);
			throw;
		}
		
	}
}