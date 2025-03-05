using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.Statistics;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankReportService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    ILoggerService logger) : IBankReportService
{
    public async Task<Result<BankManagerReport>> GetBankManagerReportAsync()
    {
        try
        {
            var userStatsTask = await GetUserStatisticsAsync();
            var transactionStatsTask = await GetTransactionStatisticsAsync();
            var dailyTransactionsTask = await GetDailyTransactionsAsync();
            var atmStatsTask = await GetAtmTransactionsStatisticsAsync();


            var report = new BankManagerReport
            {
                UserStats = userStatsTask.Value,
                TransactionStats = transactionStatsTask.Value,
                DailyTransactions = dailyTransactionsTask.Value.ToList(),
                AtmStats = atmStatsTask.Value,
            };

            return Result<BankManagerReport>.Success(report);
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating bank manager report\n" + ex);
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

            var stats = new UserStatistics
            {
                TotalUsers = await unitOfWork.BankReportRepository.GetUserCountAsync(),
                RegisteredThisYear = await unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear),
                RegisteredLastYear = await unitOfWork.BankReportRepository.GetUserCountAsync(startOfLastYear) -
                                     await unitOfWork.BankReportRepository.GetUserCountAsync(startOfThisYear),
                RegisteredLast30Days = await unitOfWork.BankReportRepository.GetUserCountAsync(thirtyDaysAgo)
            };

            return Result<UserStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating user statistics\n" + ex);
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

            var stats = new TransactionStatistics
            {
                TransactionsLastMonth = await unitOfWork.BankReportRepository.GetTransactionCountAsync(oneMonthAgo),
                TransactionsLastSixMonths = await unitOfWork.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo),
                TransactionsLastYear = await unitOfWork.BankReportRepository.GetTransactionCountAsync(oneYearAgo),
                IncomeLastMonth = await unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo),
                IncomeLastSixMonths = await unitOfWork.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo),
                IncomeLastYear = await unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo),
                AverageIncomePerTransaction = await unitOfWork.BankReportRepository.GetAverageTransactionIncomeAsync()
            };

            return Result<TransactionStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating transaction statistics\n" + ex);
            return Result<TransactionStatistics>.Failure(CustomError.Failure("Error generating transaction statistics"));
        }
    }

    public async Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30)
    {
        var data = await unitOfWork.BankReportRepository.GetDailyTransactionsAsync(days);
        return Result<IEnumerable<DailyTransactionReport>>.Success(data);
    }

    public async Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync()
    {
        try
        {
            var stats = new AtmTransactionsStatistics
            {
                TotalWithdrawnAmount = await GetTotalWithdrawalsFromAtmInGelAsync(), 
                Currency = Currency.GEL
            };
            return Result<AtmTransactionsStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            logger.LogError("Error generating ATM withdrawal statistics\n" + ex); 
            return Result<AtmTransactionsStatistics>.Failure(CustomError.Failure("Error generating ATM withdrawal statistics"));
        }
    } 
    
    private async Task<decimal> GetTotalWithdrawalsFromAtmInGelAsync()
    {
        var transactions = await unitOfWork.BankReportRepository.GetAllAtmTransactionsAsync();

        if (!transactions.Any())
        {
            throw new Exception("No transactions found");
        }

        decimal totalInGel = 0m;

        foreach (var transaction in transactions)
        {
            decimal transactionAmount = transaction.Amount;
            var currency = transaction.Currency;

            if (currency != Currency.GEL)
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
        return 20m;
    }
}
