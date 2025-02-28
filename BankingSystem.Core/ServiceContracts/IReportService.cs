using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IReportService
{
    Task<CustomResult<int>> GetRegisteredUsersCountAsync(string? year, string? month);

    Task<CustomResult<int>> GetTransactionsCountAsync(string? year, string? month);

    Task<CustomResult<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency);

    Task<CustomResult<decimal>> GetAverageTransactionsIncomeAsync(string currency);

    Task<CustomResult<IEnumerable<DailyTransactions>>> GetTransactionsChartForLastMonthAsync();
}