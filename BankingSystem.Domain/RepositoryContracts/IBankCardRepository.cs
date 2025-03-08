using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankCardRepository : IRepositoryBase
{
    Task AddCardAsync(BankCard card, CancellationToken cancellationToken = default);
    Task UpdatePinAsync(string cardNumber, string pinCode, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetAccountByCardAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<BankCard?> GetCardAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<(string PinCode,DateTime ExpiryDate, string Cvv)?> GetCardDetailsAsync(string cardNumber, CancellationToken cancellationToken = default);
    
}