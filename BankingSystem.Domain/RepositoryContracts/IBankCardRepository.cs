using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankCardRepository : IGenericRepository<BankCard>
{
    Task UpdatePinAsync(string cardNumber, string pinCode);
    Task<decimal> GetBalanceAsync(string cardNumber);
    Task<BankAccount?> GetAccountByCardAsync(string cardNumber);
    Task<BankCard?> GetCardAsync(string cardNumber);
    Task<bool> DoesCardExistAsync(string cardNumber);
    Task<bool> IsCardExpiredAsync(string cardNumber);
    Task<bool> CheckPinCodeAsync(string cardNumber, string pinCode);
    Task<(string PinCode, string Salt, DateTime ExpiryDate, string Cvv)?> GetCardDetailsAsync(string cardNumber);
}