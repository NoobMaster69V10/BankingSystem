using BankingSystem.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Identity;

public class User : IdentityUser
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string IdNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public ICollection<BankAccount> BankAccounts { get; set; }
    public ICollection<Card> Cards { get; set; }
}
