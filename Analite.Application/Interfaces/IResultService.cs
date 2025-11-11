using Analite.Application.Dtos.Results;

namespace Analite.Application.Interfaces;

public interface IResultService
{
	Task<ConversionDto> GetConversionAsync(Guid customerId, DateTime? from, DateTime? to);//optional params

	Task<HeatmapDto> GetHeatmapAsync(long pageId, DateTime? from, DateTime? to);//optional params
}