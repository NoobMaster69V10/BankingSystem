using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Domain.UnitOfWorkContracts;
public interface IUnitOfWork : IAsyncDisposable
{
    public IPersonRepository PersonRepository { get; }
    public IAccountTransactionRepository TransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IAccountRepository AccountRepository { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
