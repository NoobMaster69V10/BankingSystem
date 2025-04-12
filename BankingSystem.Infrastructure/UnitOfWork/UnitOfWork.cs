using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    private readonly Lazy<IRefreshTokenRepository> _refreshTokenRepository;
    private readonly Lazy<IPersonRepository> _personRepository;
    private readonly Lazy<IBankTransactionRepository> _bankTransactionRepository;
    private readonly Lazy<IBankCardRepository> _bankCardRepository;
    private readonly Lazy<IBankAccountRepository> _bankAccountRepository;
    private readonly Lazy<IBankReportRepository> _bankReportRepository;

    public IRefreshTokenRepository RefreshTokenRepository => _refreshTokenRepository.Value;
    public IPersonRepository PersonRepository => _personRepository.Value;
    public IBankTransactionRepository BankTransactionRepository => _bankTransactionRepository.Value;
    public IBankCardRepository BankCardRepository => _bankCardRepository.Value;
    public IBankAccountRepository BankAccountRepository => _bankAccountRepository.Value;
    public IBankReportRepository BankReportRepository => _bankReportRepository.Value;

    public UnitOfWork(IDbConnection connection, IPersonRepository personRepository,
        IBankTransactionRepository transactionRepository, IBankCardRepository bankCardRepository,
        IBankAccountRepository bankAccountRepository, IBankReportRepository bankReportRepository, IRefreshTokenRepository refreshTokenRepository)
    {
        _connection = connection;
        _personRepository = new Lazy<IPersonRepository>(() => personRepository);
        _bankTransactionRepository = new Lazy<IBankTransactionRepository>(() => transactionRepository);
        _bankCardRepository = new Lazy<IBankCardRepository>(() => bankCardRepository);
        _bankAccountRepository = new Lazy<IBankAccountRepository>(() => bankAccountRepository);
        _bankReportRepository = new Lazy<IBankReportRepository>(() => bankReportRepository);
        _refreshTokenRepository = new Lazy<IRefreshTokenRepository>(() => refreshTokenRepository);
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
