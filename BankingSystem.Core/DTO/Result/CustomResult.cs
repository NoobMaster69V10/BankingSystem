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
        _value = default;
        if (error == CustomError.None)
        {
            throw new ArgumentException("invalid error", nameof(error));
        }
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("Value can not be accessed when IsSuccess is false");

    public CustomError Error { get; }

    public static CustomResult<T> Success(T value)
    {
        return new CustomResult<T>(value);
    }

    public static CustomResult<T> Failure(CustomError error)
    {
        return new CustomResult<T>(error);
    }
    public static implicit operator CustomResult<T>(CustomError error) =>
        new(error);

    public static implicit operator CustomResult<T>(T value) =>
        new(value);
}