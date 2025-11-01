namespace Analite.Application.Dtos.Get;

public class CustomerGetFullDto : CustomerGetDto
{
	public string SecurityStamp { get; set; } = string.Empty;
	public DateTime UpdatedAt { get; set; }
	public bool IsApproved { get; set; }
	public bool IsActive { get; set; }
}