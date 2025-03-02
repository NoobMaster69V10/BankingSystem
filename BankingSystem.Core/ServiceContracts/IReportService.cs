using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IReportService
{
    Task<Result<int>> GetRegisteredUsersCountAsync(string? dateFilter);

    Task<Result<int>> GetTransactionsCountAsync(string? dateFilter);

    Task<Result<decimal>> GetTransactionsIncomeSumAsync(string? dateFilter, string currency);

    Task<Result<decimal>> GetAverageTransactionsIncomeAsync(string currency);

    Task<Result<IEnumerable<DailyTransaction>>> GetTransactionsChartForLastMonthAsync();
}