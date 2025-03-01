using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.IdentityModel.Tokens;

namespace BankingSystem.Core.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<CustomResult<int>> GetRegisteredUsersCountAsync(string? year, string? month)
    {
        switch (year)
        {
            case "current":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersThisYearAsync();
                return CustomResult<int>.Success(count);
            }
            case "previous":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastYearAsync();
                return CustomResult<int>.Success(count);
            }
        }

        if (month == "previous")
        {
            var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastMonthAsync();
            return CustomResult<int>.Success(count);
        }

        return CustomResult<int>.Failure(new CustomError("InvalidParams","Invalid Parameters"));
    }

    public async Task<CustomResult<int>> GetTransactionsCountAsync(string? year, string? month)
    {
         switch (year)
         {
             case "previous":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastYearAsync();
                 return CustomResult<int>.Success(count);
             }
             case "half":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastHalfYearAsync();
                 return CustomResult<int>.Success(count);
             }
         }

         if (month == "previous")
         { 
             var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastMonthAsync();
             return CustomResult<int>.Success(count);
         }

         return CustomResult<int>.Failure(new CustomError("InvalidParams","Invalid Parameters"));
    }

    public async Task<CustomResult<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency)
    {
        switch (year)
        {
            case "previous":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastYearAsync(currency);
                if (income is null)
                {
                    return CustomResult<decimal>.Failure(CustomError.NotFound("Income not found"));
                }
                return CustomResult<decimal>.Success((decimal)income);
            }
            case "half":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastHalfYearAsync(currency);
                if (income is null)
                {
                    return CustomResult<decimal>.Failure(CustomError.NotFound("Income not found"));
                }
                return CustomResult<decimal>.Success((decimal)income);
            }
        }

        if (month == "previous")
        {
            var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastMonthAsync(currency);
            if (income is null)
            {
                return CustomResult<decimal>.Failure(CustomError.NotFound("Income not found"));
            }
            return CustomResult<decimal>.Success((decimal)income);
        }

        return CustomResult<decimal>.Failure(new CustomError("InvalidParams","Invalid Parameters"));
    }

    public async Task<CustomResult<decimal>> GetAverageTransactionsIncomeAsync(string currency)
    {
        var averageIncome = await unitOfWork.ReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency);
        if (averageIncome is null)
        {
            return CustomResult<decimal>.Failure(CustomError.NotFound("Income not found"));
        }

        return CustomResult<decimal>.Success((decimal)averageIncome);
    }

    public async Task<CustomResult<IEnumerable<DailyTransactions>>> GetTransactionsChartForLastMonthAsync()
    {
        var transactions = await unitOfWork.ReportRepository.GetTransactionsChartForLastMonthAsync();

        if (transactions.IsNullOrEmpty())
        {
            return CustomResult<IEnumerable<DailyTransactions>>.Failure(CustomError.NotFound("No transactions found"));
        }
        
        return CustomResult<IEnumerable<DailyTransactions>>.Success(transactions);
    }
}