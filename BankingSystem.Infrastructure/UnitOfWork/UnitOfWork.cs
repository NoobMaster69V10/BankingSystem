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