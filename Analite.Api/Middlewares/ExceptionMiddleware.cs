using Analite.Domain.Exceptions;

namespace Analite.Api.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
	private readonly ILogger<ExceptionMiddleware> _logger;

	public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
	{
		_logger = logger;
	}
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch(WebException ex)
		{
			_logger.LogWarning(ex,
				"WebException: {Message}. Path: {Path}, Method: {Method}, Status: {StatusCode}",
				ex.Message,
				context.Request.Path,
				context.Request.Method,
				ex.StatusCode);
			
			
			if(!context.Response.HasStarted)
				context.Response.StatusCode = ex.StatusCode;
			await context.Response.WriteAsJsonAsync(new { error = ex.Message });
		}
		catch(Exception ex)
		{
			_logger.LogError(ex,
				"UnHandledException. Path: {Path}, Method: {Method}, User: {User}",
				context.Request.Path,
				context.Request.Method,
				context.User?.Identity?.Name ?? "anonymous");
			
			
			if(!context.Response.HasStarted)
				context.Response.StatusCode = 500;
			await context.Response.WriteAsJsonAsync(new { error = ex.Message });
		}
	}
}
