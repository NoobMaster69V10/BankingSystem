using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Errors;

public class Error
{
    private Error(
        string description,
        ErrorType errorType
    )
    {
        Description = description;
        ErrorType = errorType;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType ErrorType { get; }

    public static Error Failure(string description) =>
        new(description, ErrorType.Failure);

    public static Error NotFound(string description) =>
        new(description, ErrorType.NotFound);

    public static Error Validation(string description) =>
        new(description, ErrorType.Validation);

    public static Error Conflict(string description) =>
        new(description, ErrorType.Conflict);

    public static Error AccessUnAuthorized(string description) =>
        new(description, ErrorType.AccessUnAuthorized);

    public static Error AccessForbidden(string description) =>
        new(description, ErrorType.AccessForbidden);

    public static Error BadRequest(string description) =>
        new(description, ErrorType.BadRequest);
    public static Error ServerError(string description) =>
        new(description, ErrorType.BadRequest);
}