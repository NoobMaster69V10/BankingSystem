using System.Text.Json.Serialization;
using BankingSystem.Domain.Errors;

namespace BankingSystem.Core.DTO.Result;

public class CustomResult<T>
{
    private readonly T? _value;

    private CustomResult(T value)
    {
        Value = value;
        IsSuccess = true;
        // Error = CustomError.None;
    }

    private CustomResult(CustomError error)
    {
        if (error == CustomError.None)
        {
            throw new ArgumentException("invalid error", nameof(error));
        }

        IsSuccess = false;
        Error = error;
    }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsSuccess { get; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFailure => !IsSuccess;

    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("there is no value for failure");
            }

            return _value!;
        }

        private init => _value = value;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomError? Error { get; } 

    public static CustomResult<T> Success(T value)
    {
        return new CustomResult<T>(value);
    }

    public static CustomResult<T> Failure(CustomError error)
    {
        return new CustomResult<T>(error);
    }
}