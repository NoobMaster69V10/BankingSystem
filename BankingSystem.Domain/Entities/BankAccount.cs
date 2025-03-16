using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class BankAccount
{
    public int BankAccountId { get; set; }

    [StringLength(34, MinimumLength = 15)]
    public string? Iban { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    [StringLength(3, MinimumLength = 3)]
    public Currency Currency { get; set; }

    [JsonIgnore]
    public string PersonId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}