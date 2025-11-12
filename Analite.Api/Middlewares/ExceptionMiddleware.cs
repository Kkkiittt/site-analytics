using Analite.Domain.Exceptions;

namespace Analite.Api.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch(WebException ex)
		{
			if(!context.Response.HasStarted)
				context.Response.StatusCode = ex.StatusCode;
			await context.Response.WriteAsJsonAsync(new { error = ex.Message });
		}
		catch(Exception ex)
		{
			if(!context.Response.HasStarted)
				context.Response.StatusCode = 500;
			await context.Response.WriteAsJsonAsync(new { error = ex.Message });
		}
	}
}
