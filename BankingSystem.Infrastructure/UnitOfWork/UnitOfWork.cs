﻿using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public IPersonRepository PersonRepository { get; }
    public IBankTransactionRepository BankTransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IBankAccountRepository BankAccountRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    public IBankReportRepository BankReportRepository { get; }

    public UnitOfWork(IDbConnection connection, IPersonRepository personRepository,
        IBankTransactionRepository transactionRepository, IBankCardRepository bankCardRepository,
        IBankAccountRepository bankAccountRepository, IBankReportRepository bankReportRepository, IRefreshTokenRepository refreshTokenRepository)
    {
        _connection = connection;
        PersonRepository = personRepository;
        BankTransactionRepository = transactionRepository;
        BankCardRepository = bankCardRepository;
        BankAccountRepository = bankAccountRepository;
        BankReportRepository = bankReportRepository;
        RefreshTokenRepository = refreshTokenRepository;
        _connection.Open();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _transaction = await Task.Run(() => _connection.BeginTransaction(), cancellationToken);
            PersonRepository.SetTransaction(_transaction);
            BankTransactionRepository.SetTransaction(_transaction);
            BankCardRepository.SetTransaction(_transaction);
            BankAccountRepository.SetTransaction(_transaction);
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await Task.Run(() => _transaction.Commit(), cancellationToken);
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await Task.Run(() => _transaction.Rollback());
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        await Task.Run(() => _transaction?.Dispose());
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await RollbackAsync();
            }

            _connection.Close();
            _connection.Dispose();
            _disposed = true;
        }
    }
}
