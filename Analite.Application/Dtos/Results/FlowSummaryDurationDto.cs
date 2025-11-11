namespace Analite.Application.Dtos.Results;

public class FlowSummaryDurationDto
{

	public FlowShortDto Minimum { get; set; } = new FlowShortDto();
	public FlowShortDto Maximum { get; set; } = new FlowShortDto();
	public TimeSpan AverageDuration { get; set; }
}