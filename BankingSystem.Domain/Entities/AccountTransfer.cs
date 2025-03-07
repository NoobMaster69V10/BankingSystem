using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class AccountTransfer : BankTransactionBase
{
    public override TransactionType TransactionType => TransactionType.AccountTransfer;
    public int ToAccountId { get; set; } 
}