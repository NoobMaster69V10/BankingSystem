using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.IdentityModel.Tokens;

namespace BankingSystem.Core.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<Result<int>> GetRegisteredUsersCountAsync(string? year, string? month)
    {
        switch (year)
        {
            case "current":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersThisYearAsync();
                return Result<int>.Success(count);
            }
            case "previous":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastYearAsync();
                return Result<int>.Success(count);
                }
        }

        if (month == "previous")
        {
            var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastMonthAsync();
            return Result<int>.Success(count);
        }

        return Result<int>.Failure(CustomError.Validation("Invalid query parameters. Use ?year=current, ?year=previous, or ?month=previous"));
    }

    public async Task<Result<int>> GetTransactionsCountAsync(string? year, string? month)
    {
         switch (year)
         {
             case "previous":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastYearAsync();
                 return Result<int>.Success(count);
                }
             case "half":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastHalfYearAsync();
                 return Result<int>.Success(count);
                }
         }

         if (month == "previous")
         { 
             var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastMonthAsync();
             return Result<int>.Success(count);
         }

         return Result<int>.Failure(CustomError.Validation("Invalid query parameters. Use ?year=previous, ?year=half, or ?month=previous"));
    }

    public async Task<Result<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency)
    {
        switch (year)
        {
            case "previous":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastYearAsync(currency);
                if (income is null)
                {
                    return Result<decimal>.Failure(CustomError.NotFound("Cannot find records with these params!"));
                }
                return Result<decimal>.Success((decimal)income);
            }
            case "half":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastHalfYearAsync(currency);
                if (income is null)
                {
                    return Result<decimal>.Failure(CustomError.NotFound("Cannot find records with these params!"));
                }
                return Result<decimal>.Success((decimal)income);
                }
        }

        if (month == "previous")
        {
            var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastMonthAsync(currency);
            if (income is null)
            {
                return Result<decimal>.Failure(CustomError.NotFound("Cannot find records with these params!"));
            }
            return Result<decimal>.Success((decimal)income);
        }

        return Result<decimal>.Failure(CustomError.Validation("Invalid query parameters. Use ?year=previous, ?year=half, or ?month=previous, ?currency=GEL,USD,EUR"));

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