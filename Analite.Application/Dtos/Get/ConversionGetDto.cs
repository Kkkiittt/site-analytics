namespace Analite.Application.Dtos.Get;

public class ConversionGetDto
{
	public DateTime From { get; set; }
	public DateTime To { get; set; }

	public IEnumerable<ClickDto> Pages { get; set; } = new List<ClickDto>();
}