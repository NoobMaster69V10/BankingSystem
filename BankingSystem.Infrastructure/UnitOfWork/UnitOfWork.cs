using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Logging;

namespace BankingSystem.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbTransaction _dbTransaction;
        private readonly ILogger<UnitOfWork> _logger;

        public IPersonRepository PersonRepository { get; }
        public IAccountTransactionRepository TransactionRepository { get; }
        public IBankCardRepository BankCardRepository { get; }
        public IBankAccountRepository BankAccountRepository { get; }
        public IReportRepository ReportRepository { get; }

        public UnitOfWork(IDbTransaction dbTransaction, 
            IPersonRepository personRepository,
            IAccountTransactionRepository transactionRepository, 
            IBankCardRepository bankCardRepository,
            IBankAccountRepository bankAccountRepository, 
            IReportRepository reportRepository,
            ILogger<UnitOfWork> logger)
        {
            PersonRepository = personRepository;
            TransactionRepository = transactionRepository;
            BankCardRepository = bankCardRepository;
            BankAccountRepository = bankAccountRepository;
            ReportRepository = reportRepository;
            _dbTransaction = dbTransaction;
            _logger = logger;
        }
        public async Task CommitAsync()
        {
            try
            {
                await Task.Run(() => _dbTransaction.Commit());
                _dbTransaction.Connection.BeginTransaction();  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction.");
                await RollbackAsync();  
                throw;  
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                await Task.Run(() => _dbTransaction.Rollback());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction.");
            }
        }
        public void Dispose()
        {
            _dbTransaction.Connection?.Close();
            _dbTransaction.Connection?.Dispose();
            _dbTransaction.Dispose();
        }
    }
}
