namespace BankingSystem.Core.DTO.Response;

public class AuthenticationResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } =string.Empty;
    public string? ErrorMessage { get; set; }
}