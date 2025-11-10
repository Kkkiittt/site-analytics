namespace Analite.Application.Dtos.Get;

public class FlowGetDto
{
	public string Id { get; set; } = string.Empty;
	public DateTime StartAt { get; set; }
	public DateTime EndAt { get; set; }

	public List<ShortDto> Blocks { get; set; } = new();
	public List<ShortDto> Pages { get; set; } = new();
}