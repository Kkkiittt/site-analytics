namespace Analite.Application.Dtos.Results;

public class FlowSummaryDto
{
	public FlowShortDto Minimum { get; set; } = new FlowShortDto();
	public FlowShortDto Maximum { get; set; } = new FlowShortDto();
	public FlowShortDto Average { get; set; } = new FlowShortDto();
}

public enum SummaryType
{
	ByLength,
	ByTime
}