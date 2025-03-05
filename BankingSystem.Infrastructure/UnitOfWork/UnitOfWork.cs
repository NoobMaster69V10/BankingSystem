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
    public IBankTransactionRepository BankTransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IBankAccountRepository BankAccountRepository { get; }
    public IBankReportRepository BankReportRepository { get; }

    public UnitOfWork(IDbConnection connection, IPersonRepository personRepository,
        IBankTransactionRepository transactionRepository, IBankCardRepository bankCardRepository,
        IBankAccountRepository bankAccountRepository, IBankReportRepository bankReportRepository)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        PersonRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        BankTransactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        BankCardRepository = bankCardRepository ?? throw new ArgumentNullException(nameof(bankCardRepository));
        BankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        BankReportRepository = bankReportRepository ?? throw new ArgumentNullException(nameof(bankReportRepository));
        _connection.Open();
    }

    public Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = _connection.BeginTransaction();
            PersonRepository.SetTransaction(_transaction);
            BankTransactionRepository.SetTransaction(_transaction);
            BankCardRepository.SetTransaction(_transaction);
            BankAccountRepository.SetTransaction(_transaction);
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