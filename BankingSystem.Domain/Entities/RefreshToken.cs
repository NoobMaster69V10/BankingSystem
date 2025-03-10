namespace BankingSystem.Domain.Entities;

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; set; }
    public string PersonId { get; set; }
}