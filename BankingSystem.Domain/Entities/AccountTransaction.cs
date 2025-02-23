using System.Text.Json.Serialization;

namespace BankingSystem.Domain.Entities;

public class AccountTransaction
{
    [JsonIgnore]
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal TransactionFee { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}