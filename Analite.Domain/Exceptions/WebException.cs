namespace Analite.Domain.Exceptions;

public class WebException : Exception
{
	public int StatusCode { get; set; }

	public WebException(string message, int code = 400) : base(message)
	{
		StatusCode = code;
	}
}