
using System.Text.Json;

using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class FlowService : IFlowService
{
	private readonly AppDbContext _db;
	private readonly IDistributedCache _cache;
	private readonly ILogger<FlowService> _log;


	public FlowService(AppDbContext db, IDistributedCache cache,  ILogger<FlowService> log)
	{
		_db = db;
		_cache = cache;
		_log = log;
	}

	public async Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid customerId, DateTime? from, DateTime? to, PaginationData pagination)
	{
		_log.LogDebug("[{Time}] Fetching flows for customer {CustomerId}, Page {Page}, PageSize {PageSize}, From {From}, To {To}",
			DateTime.UtcNow,
			customerId,
			pagination.Page,
			pagination.PageSize,
			from,
			to);
		
		int skip = (pagination.Page - 1) * pagination.PageSize;

		try
		{
			var query = _db.Flows.Where(f => f.CustomerId == customerId);
			if (from.HasValue)
			{
				query = query.Where(f => f.StartAt >= from.Value);
				_log.LogDebug("Filtering flows by start >= {From}", from);
			}

			if (to.HasValue)
			{
				query = query.Where(f => f.EndAt <= to.Value);
				_log.LogDebug("Filtering flows by end <= {To}", to);
			}

			var total = await query.CountAsync();
			
			_log.LogInformation("Total flows found for customer {CustomerId}: {Total}",
				customerId,
				total);
			
			var res = await query.OrderByDescending(f => f.StartAt)
				.Skip(skip)
				.Take(pagination.PageSize)
				.Select(f =>

					new
					{
						StartAt = f.StartAt,
						Blocks = f.BlockIds,
						Pages = f.PageIds,
						EndAt = f.EndAt,
						Id = f.SessionId.ToString(),
					}
				).ToListAsync();
			var blockIds = res.SelectMany(r => r.Blocks).Distinct().ToList();
			var blocks = await _db.Blocks
				.Where(b => blockIds.Contains(b.Id))
				.Select(b => new
				{
					b.Id,
					b.Name,
					b.PageId,
					PageName = b.Page.Name
				})
				.ToDictionaryAsync(b => b.Id, b => b);
			var pageIds = res.SelectMany(r => r.Pages).Distinct().ToList();
			var pages = await _db.Pages
				.Where(p => pageIds.Contains(p.Id))
				.ToDictionaryAsync(p => p.Id, p => p.Name);
			var items = res.Select(r => new FlowGetDto()
			{
				StartAt = r.StartAt,
				EndAt = r.EndAt,
				Id = r.Id,
				Blocks = r.Blocks.Select(bid => new ShortDto()
				{
					Id = bid.ToString(),
					Name = blocks.ContainsKey(bid) ? blocks[bid].Name : "Unknown",
				}).ToList(),
				Pages = r.Pages.Select(pid => new ShortDto()
				{
					Id = pid.ToString(),
					Name = pages.ContainsKey(pid) ? pages[pid] : "Unknown",
				}).ToList(),
			}).ToList();
			
			_log.LogInformation("Retrieved {Count} flows for customer {CustomerId}",
				items.Count,
				customerId);
			
			return new ManyDto<FlowGetDto>()
			{
				Total = total,
				Items = items,
				Pagination = pagination,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error while fetching flows fro customer {CustomerId}", customerId);
			throw;
		}
		
		
	}

	public async Task<IEnumerable<FlowGetDto>> GetFlowsInCacheAsync(Guid customerId, int limit)
	{
		_log.LogDebug("[{Time}] Fetching last {Limit} cached flows for customer {CustomerId}",
			DateTime.UtcNow,
			limit,
			customerId);

		try
		{
			string? value = await _cache.GetStringAsync(customerId.ToString());
			if (value == null)
			{
				_log.LogDebug("No cached flows for customer {CustomerId}", customerId);
				return Enumerable.Empty<FlowGetDto>();
			}
			var flows = JsonSerializer.Deserialize<List<FlowGetDto>>(value) ?? new();
			
			_log.LogInformation("Cached flows found: {Count} for customer {CustomerId}",
				flows.Count,
				customerId);

			return flows.Take(limit);
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Failed to load cached flows for customer {CustomerId}",
				customerId);
			throw;
		}
		
	}

	public async Task<FlowSummaryLengthDto> GetFlowSummaryByLengthAsync(Guid customerId, DateTime? from, DateTime? to)
	{
		_log.LogDebug("Calculating flow length summary for CustomerId {CustomerId}, From {From}, To {To}",
			customerId,
			from,
			to);

		try
		{
			var query = _db.Flows.Where(f => f.CustomerId == customerId);
			if (from.HasValue)
			{
				query = query.Where(f => f.StartAt >= from.Value);
			}

			if (to.HasValue)
			{
				query = query.Where(f => f.EndAt <= to.Value);
			}

			var min = await query.OrderBy(f => f.PageIds.Count).Select(f => new FlowShortDto()
			{
				SessionId = f.SessionId.ToString(),
				PageCount = f.PageIds.Count,
				From = f.StartAt,
				To = f.EndAt,
			}).FirstOrDefaultAsync();
			var max = await query.OrderByDescending(f => f.PageIds.Count).Select(f => new FlowShortDto()
			{
				SessionId = f.SessionId.ToString(),
				PageCount = f.PageIds.Count,
				From = f.StartAt,
				To = f.EndAt,
			}).FirstOrDefaultAsync();
			
			var average = await query.AverageAsync(f => f.PageIds.Count);
			if (min == null || max == null)
			{
				_log.LogWarning("No flows found for summary calculation. CustomerId {CustomerId}", customerId);
				throw new NotFoundException("Flows in given range");
			}

			_log.LogInformation("Summary by length calculated for customer {CustomerId}: min={Min}, max={Max}, avg={Avg}",
				customerId,
				min.PageCount,
				max.PageCount,
				average);

			return new FlowSummaryLengthDto
			{
				Minimum = min,
				Maximum = max,
				AverageLength = (float)average,
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error while calculating flow summary by length for customer {CustomerId}",
				customerId);
			throw;
		}
		
	}

	public async Task<FlowSummaryDurationDto> GetFlowSummaryByDurationAsync(Guid customerId, DateTime? from, DateTime? to)
	{
		_log.LogDebug("Calculating flow duration summary for customer {CustomerId}, From {From}, To {To}",
			customerId,
			from,
			to);

		try
		{
			var query = _db.Flows.Where(f => f.CustomerId == customerId);
			if(from.HasValue)
			{
				query = query.Where(f => f.StartAt >= from.Value);
			}
			if(to.HasValue)
			{
				query = query.Where(f => f.EndAt <= to.Value);
			}
			var min = await query.OrderBy(f => f.EndAt - f.StartAt).Select(f => new FlowShortDto()
			{
				SessionId = f.SessionId.ToString(),
				PageCount = f.PageIds.Count,
				From = f.StartAt,
				To = f.EndAt,
			}).FirstOrDefaultAsync();
			var max = await query.OrderByDescending(f => f.EndAt - f.StartAt).Select(f => new FlowShortDto()
			{
				SessionId = f.SessionId.ToString(),
				PageCount = f.PageIds.Count,
				From = f.StartAt,
				To = f.EndAt,
			}).FirstOrDefaultAsync();
			
			var average = await query.AverageAsync(f => (f.EndAt - f.StartAt).Ticks);
			
			if (min == null || max == null)
			{
				_log.LogWarning("No flows found for duration summary. CustomerId {CustomerId}", customerId);
				throw new NotFoundException("Flows in given range");
			}

			_log.LogInformation("Summary by duration calculated for customer {CustomerId}: min={Min}, max={Max}, avg={Avg}",
				customerId,
				min.To - min.From,
				max.To - max.From,
				TimeSpan.FromTicks((long)average));
			
			return new FlowSummaryDurationDto()
			{
				Minimum = min,
				Maximum = max,
				AverageDuration = TimeSpan.FromTicks((long)average),
			};
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error while calculating duration summary for customer {CustomerId}",
				customerId);
			throw;
		}
		
	}
}