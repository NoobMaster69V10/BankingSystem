namespace BankingSystem.Core.ServiceContracts;

public interface ILoggerService
{
    void LogError(string errorMessage);
    void LogSuccess(string message);
}