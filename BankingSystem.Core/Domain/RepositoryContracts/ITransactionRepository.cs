using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.DTO;

namespace BankingSystem.Core.Domain.RepositoryContracts;

public interface ITransactionRepository
{
    Task TransactionBetweenOwnAccountsAsync(Transaction transaction);
    Task TransactionBetweenAnotherBankAccountsAsync(Transaction transaction);
}