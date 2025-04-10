﻿using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankAccountRepository : IRepositoryBase
{
    Task AddBankAccountAsync(BankAccount account, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default);
    Task UpdateBalanceAsync(BankAccount? account, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetAccountByIbanAsync(string iban, CancellationToken cancellationToken = default);
    Task RemoveBankAccountAsync(string iban, CancellationToken cancellationToken = default);
}
