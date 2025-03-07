using System.Text.Json.Serialization;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public abstract class BankTransactionBase
{
    [JsonIgnore]
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal TransactionFee { get; set; }
    public abstract TransactionType TransactionType { get; } 
    public int FromAccountId { get; set; } 
    public Currency Currency { get; set; }
}