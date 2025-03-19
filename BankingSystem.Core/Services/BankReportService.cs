using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.Statistics;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankReportService : IBankReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrencyExchangeClient _currencyExchangeClient;
    private readonly ILoggerService _logger;

    public BankReportService(IUnitOfWork unitOfWork, ICurrencyExchangeClient currencyExchangeClient, ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _currencyExchangeClient = currencyExchangeClient;
        _logger = logger;
    }

    public async Task<Result<BankManagerReport>> GetBankManagerReportAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userStatsTask =  GetUserStatisticsAsync(cancellationToken);
            var transactionStatsTask =  GetTransactionStatisticsAsync(cancellationToken);
            var dailyTransactionsTask =  GetDailyTransactionsAsync(cancellationToken: cancellationToken);
            var atmStatsTask =  GetAtmTransactionsStatisticsAsync(cancellationToken);
            
            await Task.WhenAll(userStatsTask, transactionStatsTask, dailyTransactionsTask, atmStatsTask);
            
            {
                var report = new BankManagerReport
                {
                    UserStats = userStatsTask.Result.Value!,
                    TransactionStats = transactionStatsTask.Result.Value!,
                    DailyTransactions = dailyTransactionsTask.Result.Value!.ToList(),
                    AtmStats = atmStatsTask.Result.Value!
                };

                return Result<BankManagerReport>.Success(report);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating bank manager report\n" + ex);
            return Result<BankManagerReport>.Failure(CustomError.Failure("Error generating bank manager report"));
        }
    }

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.Now;
            var startOfThisYear = new DateTime(now.Year, 1, 1);
            var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
            var thirtyDaysAgo = now.AddDays(-30);

            var totalUsers = _unitOfWork.BankReportRepository.GetUserCountAsync(cancellationToken: cancellationToken);
            var registeredThisYear = _unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear, cancellationToken);
            var registeredLastYear = _unitOfWork.BankReportRepository.GetUserCountAsync(startOfLastYear, cancellationToken);
            var registeredLast30Days = _unitOfWork.BankReportRepository.GetUserCountAsync(thirtyDaysAgo, cancellationToken);

            await Task.WhenAll(totalUsers, registeredLastYear, registeredLastYear, registeredLast30Days,
                registeredThisYear);


            var stats = new UserStatistics
            {
                TotalUsers = totalUsers.Result,
                RegisteredThisYear = registeredThisYear.Result,
                RegisteredLastYear = registeredLastYear.Result - registeredThisYear.Result,
                RegisteredLast30Days = registeredLast30Days.Result
            };

            return Result<UserStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating user statistics\n" + ex);
            return Result<UserStatistics>.Failure(CustomError.Failure("Error generating user statistics"));
        }
    }
    
      public async Task<Result<TransactionStatistics>> GetTransactionStatisticsAsync(CancellationToken cancellationToken = default)
      {
        try
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);
            var sixMonthsAgo = now.AddMonths(-6);
            var oneYearAgo = now.AddYears(-1);

            var transactionsLastMonthTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(oneMonthAgo, cancellationToken);
            var transactionsLastSixMonthsTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo, cancellationToken);
            var transactionsLastYearTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(oneYearAgo, cancellationToken);
            var incomeLastMonthTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo, cancellationToken);
            var incomeLastSixMonthsTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo, cancellationToken);
            var incomeLastYearTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo, cancellationToken);
            var avgIncomePerTransactionTask = _unitOfWork.BankReportRepository.GetAverageTransactionIncomeAsync(cancellationToken: cancellationToken);

            await Task.WhenAll(transactionsLastMonthTask, transactionsLastSixMonthsTask, transactionsLastYearTask,
                               incomeLastMonthTask, incomeLastSixMonthsTask, incomeLastYearTask, avgIncomePerTransactionTask);

            var stats = new TransactionStatistics
            {
                TransactionsLastMonth = transactionsLastMonthTask.Result,
                TransactionsLastSixMonths = transactionsLastSixMonthsTask.Result,
                TransactionsLastYear = transactionsLastYearTask.Result,
                IncomeLastMonth = incomeLastMonthTask.Result,
                IncomeLastSixMonths = incomeLastSixMonthsTask.Result,
                IncomeLastYear = incomeLastYearTask.Result,
                AverageIncomePerTransaction = avgIncomePerTransactionTask.Result
            };

            return Result<TransactionStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating transaction statistics\n" + ex);
            return Result<TransactionStatistics>.Failure(CustomError.Failure("Error generating transaction statistics"));
        }
      }


    public async Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var data = await _unitOfWork.BankReportRepository.GetDailyTransactionsAsync(days, cancellationToken);
        return Result<IEnumerable<DailyTransactionReport>>.Success(data);
    }

    public async Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new AtmTransactionsStatistics
            {
                TotalWithdrawnAmount = await GetTotalWithdrawalsFromAtmInGelAsync(cancellationToken), 
                Currency = Currency.GEL.ToString()
            };
            return Result<AtmTransactionsStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating ATM withdrawal statistics\n" + ex); 
            return Result<AtmTransactionsStatistics>.Failure(CustomError.Failure("Error generating ATM withdrawal statistics"));
        }
    } 
        
    private async Task<decimal> GetTotalWithdrawalsFromAtmInGelAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _unitOfWork.BankReportRepository.GetAllAtmTransactionsAsync(cancellationToken);

        var atmTransactions = transactions as AtmTransaction[] ?? transactions.ToArray();

        if (!atmTransactions.Any())
        {
            return 0;
        }
    
        var totalInGel = 0m;
    
        foreach (var transaction in atmTransactions)
        {
            var transactionAmount = transaction.Amount;
            var currency = transaction.Currency;
    
            if (currency != Currency.GEL)
            {
                var exchangeRate = await _currencyExchangeClient.GetExchangeRateAsync(currency, cancellationToken);
                if (exchangeRate <= 0)
                {
                    throw new Exception("Invalid exchange rate");
                }
    
                transactionAmount *= exchangeRate;
            }
    
            totalInGel += transactionAmount;
        }
        return totalInGel;
    }
}
