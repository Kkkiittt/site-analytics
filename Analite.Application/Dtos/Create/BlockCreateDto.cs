namespace Analite.Application.Dtos.Create;

public class BlockCreateDto
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public long PageId { get; set; }
}