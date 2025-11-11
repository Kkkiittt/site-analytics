namespace Analite.Domain.Exceptions;

public class UnauthorizedException : WebException
{
	public UnauthorizedException(string message) : base("Can't authenticate you by " + message, 401)
	{
	}
}