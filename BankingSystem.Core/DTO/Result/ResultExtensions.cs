using BankingSystem.Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.Core.DTO.Result;

public static class ResultExtensions
{
    public static IActionResult ToProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Result is success");
        }

        return new ObjectResult(new ProblemDetails
        {
            Status = GetStatusCode(result.Error.ErrorType),
            Title = GetTitle(result.Error.ErrorType),
            Type = GetType(result.Error.ErrorType),
            Extensions = { { "errors", new[] { result.Error } } }
        })
        {
            StatusCode = GetStatusCode(result.Error.ErrorType)
        };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.AccessForbidden => StatusCodes.Status403Forbidden,
                ErrorType.AccessUnAuthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

        static string GetTitle(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "Validation",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                ErrorType.AccessForbidden => "Access Forbidden",
                ErrorType.AccessUnAuthorized => "Access UnAuthorized",
                ErrorType.Failure => "Failure",
                _ => "Server Failure"
            };


        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1", 
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4", 
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8", 
                ErrorType.AccessForbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3", 
                ErrorType.AccessUnAuthorized => "https://tools.ietf.org/html/rfc7235#section-3.1", 
                ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.6.1", 
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1" 
            };
    }
}