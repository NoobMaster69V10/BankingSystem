using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.Statistics;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sprache;

namespace BankingSystem.Core.Services;

public class BankReportService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    IMemoryCache cache,
    ILogger<BankReportService> logger) : IBankReportService
{
    
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<BankManagerReport>> GetBankManagerReportAsync()
    {
        var cacheKey = "BankManagerReport";
    
        if (cache.TryGetValue(cacheKey, out BankManagerReport? cachedReport))
        {
            return Result<BankManagerReport>.Success(cachedReport);
        }
        try
        {
            var userStats =await GetUserStatisticsAsync();
            var transactionStats = await GetTransactionStatisticsAsync();
            var dailyTransactions = await GetDailyTransactionsAsync();
            var atmStats = await GetAtmTransactionsStatisticsAsync();
            
            var report = new BankManagerReport
            {
                UserStats = userStats.Value,
                TransactionStats = transactionStats.Value,
                DailyTransactions = dailyTransactions.Value.ToList(),
                AtmStats = atmStats.Value,
            };
    
            cache.Set(cacheKey, report, _cacheDuration);
            return Result<BankManagerReport>.Success(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating bank manager report"); 
            return Result<BankManagerReport>.Failure(CustomError.Failure("Error generating bank manager report"));
        }
    }

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync()
    {
        var cacheKey = "UserStatistics";

        if (cache.TryGetValue(cacheKey, out UserStatistics cachedStats))
        {
            return Result<UserStatistics>.Success(cachedStats);
        }

        try
        {
            var now = DateTime.Now;
            var startOfThisYear = new DateTime(now.Year, 1, 1);
            var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
            var thirtyDaysAgo = now.AddDays(-30);

            var stats = new UserStatistics
            {
                TotalUsers = await unitOfWork.BankReportRepository.GetUserCountAsync(),
                RegisteredThisYear = await unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear),
                RegisteredLastYear = await unitOfWork.BankReportRepository.GetUserCountAsync(startOfLastYear) -
                                     await unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear),
                RegisteredLast30Days = await unitOfWork.BankReportRepository.GetUserCountAsync(thirtyDaysAgo)
            };

            cache.Set(cacheKey, stats, _cacheDuration);
            return Result<UserStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating user statistics");
            throw;
        }
    }

    public async Task<Result<TransactionStatistics>> GetTransactionStatisticsAsync()
    {
        string cacheKey = "TransactionStatistics";

        if (cache.TryGetValue(cacheKey, out TransactionStatistics cachedStats))
        {
            return Result<TransactionStatistics>.Success(cachedStats);
        }
        try
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);
            var sixMonthsAgo = now.AddMonths(-6);
            var oneYearAgo = now.AddYears(-1);

            var stats = new TransactionStatistics
            {
                TransactionsLastMonth = await unitOfWork.BankReportRepository.GetTransactionCountAsync(oneMonthAgo),
                TransactionsLastSixMonths = await  unitOfWork.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo),
                TransactionsLastYear = await  unitOfWork.BankReportRepository.GetTransactionCountAsync(oneYearAgo),

                IncomeLastMonth = await  unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo),
                IncomeLastSixMonths = await  unitOfWork.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo),
                IncomeLastYear = await  unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo),

                AverageIncomePerTransaction = await  unitOfWork.BankReportRepository.GetAverageTransactionIncomeAsync()
            };

            cache.Set(cacheKey, stats, _cacheDuration);
            return Result<TransactionStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating transaction statistics");
            return Result<TransactionStatistics>.Failure(CustomError.Failure("Error generating transaction statistics"));
        }
    }

    public async Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30)
    {
        string cacheKey = $"DailyTransactions_{days}";

        if (cache.TryGetValue(cacheKey, out IEnumerable<DailyTransactionReport> cachedData))
        {
            return Result<IEnumerable<DailyTransactionReport>>.Success(cachedData);
        }

        var data = await  unitOfWork.BankReportRepository.GetDailyTransactionsAsync(days);
        cache.Set(cacheKey, data, _cacheDuration);
        return Result<IEnumerable<DailyTransactionReport>>.Success(data);
    }

    public async Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync()
    {
        string cacheKey = "AtmWithdrawalStatistics";

        if (cache.TryGetValue(cacheKey, out AtmTransactionsStatistics cachedStats))
        {
            return Result<AtmTransactionsStatistics>.Success(cachedStats);
        }

        try
        {
            var stats = new AtmTransactionsStatistics
            {
                TotalWithdrawnAmount = await GetTotalWithdrawalsFromAtmInGelAsync(), 
            };

            cache.Set(cacheKey, stats, _cacheDuration);
            return Result<AtmTransactionsStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating ATM withdrawal statistics"); 
            return Result<AtmTransactionsStatistics>.Failure(CustomError.Failure("Error generating ATM withdrawal statistics"));
        }
    } 
    
    
    private async Task<decimal> GetTotalWithdrawalsFromAtmInGelAsync()
    {
        var transactions = await unitOfWork.AtmRepository.GetAllAtmTransactionsAsync();

        if (!transactions.Any())
        {
            throw new Exception("No transactions found");
        }

        decimal totalInGel = 0m;

        foreach (var transaction in transactions)
        {
            decimal transactionAmount = transaction.Amount;
            var currency = transaction.Currency;

            if (currency != "GEL")
            {
                var exchangeRate = await exchangeRateApi.GetExchangeRate(currency);
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