using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public IPersonRepository PersonRepository { get; }
    public IAccountTransactionRepository TransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IAccountRepository AccountRepository { get; }

    public UnitOfWork(IDbConnection connection, IPersonRepository personRepository,
        IAccountTransactionRepository transactionRepository, IBankCardRepository bankCardRepository,
        IAccountRepository accountRepository)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        PersonRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        TransactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        BankCardRepository = bankCardRepository ?? throw new ArgumentNullException(nameof(bankCardRepository));
        AccountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _connection.Open();
    }

    public Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = _connection.BeginTransaction();
            PersonRepository.SetTransaction(_transaction);
            TransactionRepository.SetTransaction(_transaction);
            BankCardRepository.SetTransaction(_transaction);
            AccountRepository.SetTransaction(_transaction);
        }

        return Task.CompletedTask;
    }

    public Task CommitAsync()
    {
        if (_transaction != null)
        {
            _transaction.Commit();
            return DisposeTransactionAsync();
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        if (_transaction != null)
        {
            _transaction.Rollback();
            return DisposeTransactionAsync();
        }

        return Task.CompletedTask;
    }

    private Task DisposeTransactionAsync()
    {
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
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