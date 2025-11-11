namespace Analite.Domain.Exceptions;

public class NotFoundException : WebException
{
	public NotFoundException(string message) : base(message + " not found", 404)
	{
	}
}