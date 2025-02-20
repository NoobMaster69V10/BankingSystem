namespace BankingSystem.Core.DTO.Response;

public class AdvancedApiResponse<T>(bool success, string message, T data)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
    public T Data { get; set; } = data;

    public static AdvancedApiResponse<T> SuccessResponse(T data, string message = "Request successful") =>
        new(true, message, data);
    public static AdvancedApiResponse<T> ErrorResponse(string message) => new(false, message, default!);
}