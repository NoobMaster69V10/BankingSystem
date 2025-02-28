using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.IdentityModel.Tokens;

namespace BankingSystem.Core.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<AdvancedApiResponse<int>> GetRegisteredUsersCountAsync(string? year, string? month)
    {
        switch (year)
        {
            case "current":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersThisYear();
                return AdvancedApiResponse<int>.SuccessResponse(count);
            }
            case "previous":
            {
                var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastYear();
                return AdvancedApiResponse<int>.SuccessResponse(count);
            }
        }

        if (month == "previous")
        {
            var count = await unitOfWork.ReportRepository.GetNumberOfRegisteredUsersForLastMonth();
            return AdvancedApiResponse<int>.SuccessResponse(count);
        }

        return AdvancedApiResponse<int>.ErrorResponse("Invalid query parameters. Use ?year=current, ?year=previous, or ?month=previous");
    }

    public async Task<AdvancedApiResponse<int>> GetTransactionsCountAsync(string? year, string? month)
    {
         switch (year)
         {
             case "previous":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastYear();
                 return AdvancedApiResponse<int>.SuccessResponse(count);
             }
             case "half":
             {
                 var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastHalfYear();
                 return AdvancedApiResponse<int>.SuccessResponse(count);
             }
         }

         if (month == "previous")
         { 
             var count = await unitOfWork.ReportRepository.GetNumberOfTransactionsForLastMonth();
             return AdvancedApiResponse<int>.SuccessResponse(count);
         }

         return AdvancedApiResponse<int>.ErrorResponse("Invalid query parameters. Use ?year=previous, ?year=half, or ?month=previous");
    }

    public async Task<AdvancedApiResponse<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency)
    {
        switch (year)
        {
            case "previous":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastYear(currency);
                if (income is null)
                {
                    return AdvancedApiResponse<decimal>.ErrorResponse("Cannot find records with these params!");
                }
                return AdvancedApiResponse<decimal>.SuccessResponse((decimal)income);
            }
            case "half":
            {
                var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastHalfYear(currency);
                if (income is null)
                {
                    return AdvancedApiResponse<decimal>.ErrorResponse("Cannot find records with these params!");
                }
                return AdvancedApiResponse<decimal>.SuccessResponse((decimal)income);
            }
        }

        if (month == "previous")
        {
            var income = await unitOfWork.ReportRepository.GetTransactionsIncomeByCurrencyLastMonth(currency);
            if (income is null)
            {
                return AdvancedApiResponse<decimal>.ErrorResponse("Cannot find records with these params!");
            }
            return AdvancedApiResponse<decimal>.SuccessResponse((decimal)income);
        }

        return AdvancedApiResponse<decimal>.ErrorResponse("Invalid query parameters. Use ?year=previous, ?year=half, or ?month=previous, ?currency=GEL,USD,EUR");

    }

    public async Task<AdvancedApiResponse<decimal>> GetAverageTransactionsIncomeAsync(string currency)
    {
        var averageIncome = await unitOfWork.ReportRepository.GetAverageTransactionsIncomeByCurrency(currency);
        if (averageIncome is null)
        {
            return AdvancedApiResponse<decimal>.ErrorResponse("Cannot find records with these params!");
        }

        return AdvancedApiResponse<decimal>.SuccessResponse((decimal)averageIncome);
    }

    public async Task<AdvancedApiResponse<IEnumerable<DailyTransactions>>> GetTransactionsChartForLastMonthAsync()
    {
        var transactions = await unitOfWork.ReportRepository.GetTransactionsChartForLastMonth();

        if (transactions.IsNullOrEmpty())
        {
            return AdvancedApiResponse<IEnumerable<DailyTransactions>>.ErrorResponse("There is not transaction");
        }
        return AdvancedApiResponse<IEnumerable<DailyTransactions>>.SuccessResponse(transactions);
    }
}