namespace Analite.Application.Dtos.Results;

public class ConversionDto
{
	public DateTime From { get; set; }
	public DateTime To { get; set; }

	public IEnumerable<ClickDto> Pages { get; set; } = new List<ClickDto>();
}