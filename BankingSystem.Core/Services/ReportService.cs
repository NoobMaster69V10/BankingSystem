using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.IdentityModel.Tokens;

namespace BankingSystem.Core.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<Result<int>> GetRegisteredUsersCountAsync(string? dateFilter)
    {
        if (dateFilter is "current-year" or "last-year" or "last-month")
        {
            var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersAsync(dateFilter);
            return Result<int>.Success(count);
        }

        return Result<int>.Failure(CustomError.Validation("Invalid query parameters. Use ?dateFilter=current-year, ?dateFilter=last-year, or ?dateFilter=last-month"));
    }

    public async Task<Result<int>> GetTransactionsCountAsync(string? dateFilter)
    {
        if (dateFilter is "last-year" or "half-year" or "last-month")
        {
            var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsAsync(dateFilter);
            return Result<int>.Success(count);
        }

        return Result<int>.Failure(CustomError.Validation("Invalid query parameters. Use ?dateFilter=last-year, ?dateFilter=half-year, or ?dateFilter=last-month"));
    }
    public async Task<Result<decimal>> GetTransactionsIncomeSumAsync(string? dateFilter, string currency)
    {
        if (dateFilter is "last-year" or "half-year" or "last-month")
        {
            var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyAsync(dateFilter, currency);
            if (income is null)
            {
                return Result<decimal>.Failure(CustomError.NotFound("Transactions with these params not found!"));
            }
            return Result<decimal>.Success((decimal)income!);
        }

        return Result<decimal>.Failure(CustomError.Validation("Invalid query parameters. Use ?dateFilter=last-year, ?dateFilter=half-year, or ?dateFilter=last-month / ?currency=GEL,USD,EUR"));
    }

    public async Task<Result<decimal>> GetAverageTransactionsIncomeAsync(string currency)
    {
        var averageIncome = await unitOfWork.ReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency);
        if (averageIncome is null)
        {
            return Result<decimal>.Failure(CustomError.NotFound("Cannot find records with these params!"));
        }

        return Result<decimal>.Success((decimal)averageIncome);
    }

    public async Task<Result<IEnumerable<DailyTransaction>>> GetTransactionsChartForLastMonthAsync()
    {
        var transactions = await unitOfWork.ReportRepository.GetTransactionsChartForLastMonthAsync();

        var dailyTransactions = transactions!.ToList();

        if (dailyTransactions.IsNullOrEmpty())
        {
            return Result<IEnumerable<DailyTransaction>>.Failure(CustomError.NotFound("There is not transactions"));
        }

        return Result<IEnumerable<DailyTransaction>>.Success(dailyTransactions);
    }
}