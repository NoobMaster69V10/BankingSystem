
namespace BankingSystem.Domain.Entities;

public class Person
{
    public string PersonId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public IList<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public IList<BankCard> Cards { get; set; } = new List<BankCard>();
}
