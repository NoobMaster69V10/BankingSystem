using BankingSystem.Core.Identity;

namespace BankingSystem.Core.Domain.Entities;

public class BankAccount
{
    public int Id { get; set; }
    public string IBAN { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}