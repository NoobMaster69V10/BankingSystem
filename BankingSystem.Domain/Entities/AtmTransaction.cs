using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class AtmTransaction : BankTransactionBase
{
    public override TransactionType TransactionType => TransactionType.Atm;
}