namespace BankingSystem.Domain.Entities;

public class BankCard
{
    public int Id { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required string CardNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public required string Cvv { get; set; }
    public required string PinCode { get; set; }

    public required string PersonId { get; set; }
    public Person? Person { get; set; }
}
