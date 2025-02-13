using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;
    private bool _disposed;

    public IPersonRepository PersonRepository { get; }

    public IAccountTransactionRepository TransactionRepository { get; }

    public IBankCardRepository BankCardRepository { get; }

    public IAccountRepository AccountRepository { get; }

    public UnitOfWork(IDbConnection connection, IPersonRepository userRepository, IAccountTransactionRepository transactionRepository, IBankCardRepository bankCardRepository, IAccountRepository accountRepository)
    {
        _connection = connection;
        _connection.Open();
        PersonRepository = userRepository;
        TransactionRepository = transactionRepository;
        BankCardRepository = bankCardRepository;
        AccountRepository = accountRepository;
    }

   

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null!)
        {
            _transaction = await Task.Run(() => _connection.BeginTransaction());
            PersonRepository.SetTransaction(_transaction);
            TransactionRepository.SetTransaction(_transaction);
            BankCardRepository.SetTransaction(_transaction);
            AccountRepository.SetTransaction(_transaction);
        }
    }

    public async Task CommitAsync()
    {
        if (_transaction != null!)
        {
            await Task.Run(() => _transaction.Commit());
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null!)
        {
            await Task.Run(() => _transaction.Rollback());
            await DisposeTransactionAsync();
        }
    }

    public async Task DisposeTransactionAsync()
    {
        await Task.Run(() =>
        {
            _transaction.Dispose();
            _transaction = null!;
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null!)
            {
                await RollbackAsync();
            }
            _connection.Close();
            _connection.Dispose();
            _disposed = true;
        }
    }
}