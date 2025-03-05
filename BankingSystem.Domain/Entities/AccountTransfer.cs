using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class AccountTransfer : BankTransaction
{
    public override TransactionType TransactionType => TransactionType.Atm;
    public int ToAccountId { get; set; } 
}