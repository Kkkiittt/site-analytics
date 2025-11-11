namespace Analite.Domain.Exceptions;

public class UsedException : WebException
{
	public UsedException(string message) : base(message + " is already used", 409)
	{
	}
}