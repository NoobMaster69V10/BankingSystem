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

    public async Task<Result<BankManagerReport>> GetBankManagerReportAsync()
    {
        try
        {
            var userStatsTask =  GetUserStatisticsAsync();
            var transactionStatsTask =  GetTransactionStatisticsAsync();
            var dailyTransactionsTask =  GetDailyTransactionsAsync();
            var atmStatsTask =  GetAtmTransactionsStatisticsAsync();
            
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

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync()
    {
        try
        {
            var now = DateTime.Now;
            var startOfThisYear = new DateTime(now.Year, 1, 1);
            var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
            var thirtyDaysAgo = now.AddDays(-30);

            var totalUsers = _unitOfWork.BankReportRepository.GetUserCountAsync();
            var registeredThisYear = _unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear);
            var registeredLastYear = _unitOfWork.BankReportRepository.GetUserCountAsync(startOfLastYear);
            var startOfThisYearCount = _unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear);
            var registeredLast30Days = _unitOfWork.BankReportRepository.GetUserCountAsync(thirtyDaysAgo);

            await Task.WhenAll(totalUsers, registeredLastYear, registeredLastYear, registeredLast30Days,
                registeredThisYear, startOfThisYearCount);


            var stats = new UserStatistics
            {
                TotalUsers = totalUsers.Result,
                RegisteredThisYear = registeredThisYear.Result,
                RegisteredLastYear = registeredLastYear.Result - startOfThisYearCount.Result,
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
    
  public async Task<Result<TransactionStatistics>> GetTransactionStatisticsAsync()
  {
    try
    {
        var now = DateTime.Now;
        var oneMonthAgo = now.AddMonths(-1);
        var sixMonthsAgo = now.AddMonths(-6);
        var oneYearAgo = now.AddYears(-1);

        var transactionsLastMonthTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(oneMonthAgo);
        var transactionsLastSixMonthsTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo);
        var transactionsLastYearTask = _unitOfWork.BankReportRepository.GetTransactionCountAsync(oneYearAgo);
        var incomeLastMonthTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo);
        var incomeLastSixMonthsTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo);
        var incomeLastYearTask = _unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo);
        var avgIncomePerTransactionTask = _unitOfWork.BankReportRepository.GetAverageTransactionIncomeAsync();

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


    public async Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30)
    {
        var data = await _unitOfWork.BankReportRepository.GetDailyTransactionsAsync(days);
        return Result<IEnumerable<DailyTransactionReport>>.Success(data);
    }

    public async Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync()
    {
        try
        {
            var stats = new AtmTransactionsStatistics
            {
                TotalWithdrawnAmount = await GetTotalWithdrawalsFromAtmInGelAsync(), 
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
    
    private async Task<decimal> GetTotalWithdrawalsFromAtmInGelAsync()
    {
        var transactions = await _unitOfWork.BankReportRepository.GetAllAtmTransactionsAsync();

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
                var exchangeRate = await _currencyExchangeClient.GetExchangeRate(currency);
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
