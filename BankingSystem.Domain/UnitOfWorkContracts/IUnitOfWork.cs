using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Domain.UnitOfWorkContracts;
public interface IUnitOfWork : IAsyncDisposable
{
    public IPersonRepository PersonRepository { get; }
    public IBankTransactionRepository BankTransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IBankAccountRepository BankAccountRepository { get; }
    public IBankReportRepository BankReportRepository { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync();
}