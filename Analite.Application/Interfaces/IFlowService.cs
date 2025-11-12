using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Dtos.Results;

namespace Analite.Application.Interfaces;

public interface IFlowService
{
	Task<FlowSummaryLengthDto> GetFlowSummaryByLengthAsync(Guid? customerId, DateTime? from, DateTime? to);

	Task<FlowSummaryDurationDto> GetFlowSummaryByDurationAsync(Guid? customerId, DateTime? from, DateTime? to);

	Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid? customerId, DateTime? from, DateTime? to, PaginationData pagination);

	Task<IEnumerable<FlowGetDto>> GetFlowsInCacheAsync(Guid? customerId, int limit);
}