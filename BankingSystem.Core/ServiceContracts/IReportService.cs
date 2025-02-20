using BankingSystem.Core.DTO.Response;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IReportService
{
    Task<AdvancedApiResponse<int>> GetRegisteredUsersCount(string? year, string? month);

    Task<AdvancedApiResponse<int>> GetTransactionsCount(string? year, string? month);

    Task<AdvancedApiResponse<decimal>> GetTransactionsIncomeSum(string? year, string? month, string currency);

    Task<AdvancedApiResponse<decimal>> GetAverageTransactionsIncome(string currency);

    Task<AdvancedApiResponse<IEnumerable<DailyTransactions>>> GetTransactionsChartForLastMonth();
}