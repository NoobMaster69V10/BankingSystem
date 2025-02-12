using System.ComponentModel.DataAnnotations.Schema;

namespace BankingSystem.Domain.Entities;

public class BankAccount
{
    public int Id { get; set; }
    public required string IBAN { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    public required string Currency { get; set; }

    public required string PersonId { get; set; }
    public Person? Person { get; set; }
}