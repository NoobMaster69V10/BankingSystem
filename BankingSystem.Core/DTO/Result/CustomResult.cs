using System.Text.Json.Serialization;
using BankingSystem.Domain.Errors;

namespace BankingSystem.Core.DTO.Result;

public class CustomResult<T>
{
    private readonly T? _value;

    private CustomResult(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = CustomError.None;
    }

    private CustomResult(CustomError error)
    {
        if (error == CustomError.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = false;
        Error = error;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsSuccess { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFailure => !IsSuccess;

    public T? Value => IsSuccess ? _value : default;  

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomError? Error { get; }

    public static CustomResult<T> Success(T value) => new(value);

    public static CustomResult<T> Failure(CustomError error) => new(error);

    public bool TryGetValue(out T? value)
    {
        value = _value;
        return IsSuccess;
    }
}