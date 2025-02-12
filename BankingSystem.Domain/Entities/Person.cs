namespace BankingSystem.Domain.Entities;
public class Person
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string IdNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public required string Email { get; set; }
    public ICollection<BankAccount>? BankAccounts { get; set; }
    public ICollection<BankCard>? Cards { get; set; }
}
