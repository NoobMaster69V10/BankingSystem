using BankingSystem.Core.DTO.Response;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IReportService
{
    Task<AdvancedApiResponse<int>> GetRegisteredUsersCountAsync(string? year, string? month);

    Task<AdvancedApiResponse<int>> GetTransactionsCountAsync(string? year, string? month);

    Task<AdvancedApiResponse<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency);

    Task<AdvancedApiResponse<decimal>> GetAverageTransactionsIncomeAsync(string currency);

    Task<AdvancedApiResponse<IEnumerable<DailyTransactions>>> GetTransactionsChartForLastMonthAsync();
}