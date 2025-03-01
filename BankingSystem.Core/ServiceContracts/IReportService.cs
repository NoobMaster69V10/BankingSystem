using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IReportService
{
    Task<Result<int>> GetRegisteredUsersCountAsync(string? year, string? month);

    Task<Result<int>> GetTransactionsCountAsync(string? year, string? month);

    Task<Result<decimal>> GetTransactionsIncomeSumAsync(string? year, string? month, string currency);

    Task<Result<decimal>> GetAverageTransactionsIncomeAsync(string currency);

    Task<Result<IEnumerable<DailyTransaction>>> GetTransactionsChartForLastMonthAsync();
}