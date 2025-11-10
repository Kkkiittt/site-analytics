namespace Analite.Application.Dtos.Results;

public class HeatmapDto
{
	public DateTime From { get; set; }
	public DateTime To { get; set; }

	public ShortDto Page { get; set; } = new ShortDto();
	public IEnumerable<ClickDto> Blocks { get; set; } = new List<ClickDto>();
}