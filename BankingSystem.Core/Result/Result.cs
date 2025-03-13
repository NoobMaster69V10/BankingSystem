using System.Text.Json.Serialization;
using BankingSystem.Domain.Errors;

namespace BankingSystem.Core.Result;

public class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = CustomError.None;
    }

    private Result(CustomError error)
    {
        if (error == CustomError.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFailure => !IsSuccess;

    public T? Value => IsSuccess ? _value : default;  

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomError? Error { get; }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(CustomError error) => new(error);

    public bool TryGetValue(out T? value)
    {
        value = _value;
        return IsSuccess;
    }
}