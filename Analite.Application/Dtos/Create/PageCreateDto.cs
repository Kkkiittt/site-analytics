namespace Analite.Application.Dtos.Create;

public class PageCreateDto
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public int Order { get; set; }

	public Guid CustomerId { get; set; }
}