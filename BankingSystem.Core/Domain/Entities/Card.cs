using BankingSystem.Core.Identity;

namespace BankingSystem.Core.Domain.Entities;

public class Card
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Lastname { get; set; }
    public string CardNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string CVV { get; set; }
    public string PinCode { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}
