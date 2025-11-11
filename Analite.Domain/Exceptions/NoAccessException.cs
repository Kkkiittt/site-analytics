namespace Analite.Domain.Exceptions;

public class NoAccessException : WebException
{
	public NoAccessException(string message) : base("No access to " + message, 403)
	{
	}
}