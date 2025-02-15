using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankCardRepository : ITransaction
{
    Task CreateCardAsync(BankCard card);
    Task<bool> ValidateCardAsync(string cardNumber,string pinCode);
    Task UpdatePinAsync(string cardNumber, string pinCode);
    
    Task<Decimal> GetBalanceAsync(string cardNumber);
}