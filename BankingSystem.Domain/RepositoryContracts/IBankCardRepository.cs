using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankCardRepository : ITransaction
{
    Task CreateCardAsync(BankCard card);
    Task<bool> ValidateCardAsync(string cardNumber,string pinCode);
    Task UpdatePinAsync(string cardNumber, string pinCode);
    Task<decimal> GetBalanceAsync(string cardNumber);
}