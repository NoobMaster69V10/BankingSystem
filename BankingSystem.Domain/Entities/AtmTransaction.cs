using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class AtmTransaction : BankTransaction
{
    public override TransactionType TransactionType => TransactionType.Atm;
}