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
            var userStatsTask =  GetUserStatisticsAsync();
            var transactionStatsTask =  GetTransactionStatisticsAsync();
            var dailyTransactionsTask =  GetDailyTransactionsAsync();
            var atmStatsTask =  GetAtmTransactionsStatisticsAsync();
            
            await Task.WhenAll(userStatsTask, transactionStatsTask, dailyTransactionsTask, atmStatsTask);
            
            {
                var report = new BankManagerReport
                {
                    UserStats = userStatsTask.Result.Value,
                    TransactionStats = transactionStatsTask.Result.Value,
                    DailyTransactions = dailyTransactionsTask.Result.Value.ToList(),
                    AtmStats = atmStatsTask.Result.Value,
                };

                return Result<BankManagerReport>.Success(report);
            }
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

        var transactionsLastMonthTask = unitOfWork.BankReportRepository.GetTransactionCountAsync(oneMonthAgo);
        var transactionsLastSixMonthsTask = unitOfWork.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo);
        var transactionsLastYearTask = unitOfWork.BankReportRepository.GetTransactionCountAsync(oneYearAgo);
        var incomeLastMonthTask = unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo);
        var incomeLastSixMonthsTask = unitOfWork.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo);
        var incomeLastYearTask = unitOfWork.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo);
        var avgIncomePerTransactionTask = unitOfWork.BankReportRepository.GetAverageTransactionIncomeAsync();

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
            return 0;
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
    }
}
