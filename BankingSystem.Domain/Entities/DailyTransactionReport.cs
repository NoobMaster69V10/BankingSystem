using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class DailyTransactionReport
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public Dictionary<Currency, decimal> TotalAmount { get; set; } = new();
}
