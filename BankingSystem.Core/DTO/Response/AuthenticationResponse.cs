namespace BankingSystem.Core.DTO.Response;

public class AuthenticationResponse
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}