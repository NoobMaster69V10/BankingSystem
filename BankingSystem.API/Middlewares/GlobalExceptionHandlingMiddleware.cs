using System.Net;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = AdvancedApiResponse<string>.ErrorResponse(ex.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}