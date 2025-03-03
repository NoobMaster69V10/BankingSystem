using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Domain.UnitOfWorkContracts;
public interface IUnitOfWork : IAsyncDisposable
{
    public IPersonRepository PersonRepository { get; }
    public IAtmRepository AtmRepository { get; }
    public IAccountTransactionRepository TransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IBankAccountRepository BankAccountRepository { get; }
    public IBankReportRepository BankReportRepository { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}