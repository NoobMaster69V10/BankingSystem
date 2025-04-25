using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Statistics;

public class TransactionCsvReport
{
    public int BankCardId { get; set; }
    public string IdNumber { get; set; }
    public string CardNumber { get; set; }
    public string IBAN { get; set; }
    public decimal Amount { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal TotalAmount { get; set; }
}