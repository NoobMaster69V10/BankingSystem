﻿namespace BankingSystem.Domain.Entities;

public class AccountTransaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public DateTime TransactionDate { get; set; }
    public int FromAccountId { get; set; }
    public BankAccount? FromAccount { get; set; }
    public int ToAccountId { get; set; }
    public BankAccount? ToAccount { get; set; }
}