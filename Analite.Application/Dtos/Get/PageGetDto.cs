namespace Analite.Application.Dtos.Get;

public class PageGetDto
{
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public int Order { get; set; }

	public List<ShortDto> Blocks { get; set; } = new();
}