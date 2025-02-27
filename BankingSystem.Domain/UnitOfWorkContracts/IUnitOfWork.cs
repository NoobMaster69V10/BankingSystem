using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Domain.UnitOfWorkContracts;
public interface IUnitOfWork : IDisposable
{
    public IPersonRepository PersonRepository { get; }
    public IAccountTransactionRepository TransactionRepository { get; }
    public IBankCardRepository BankCardRepository { get; }
    public IBankAccountRepository BankAccountRepository { get; }
    public IReportRepository ReportRepository { get; }
    Task  CommitAsync();
}
