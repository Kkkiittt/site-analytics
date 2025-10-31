namespace Analite.Application.Dtos.Get;

public class BlockGetDto
{
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public long PageId { get; set; }
	public string PageName{ get; set; } = string.Empty;
}