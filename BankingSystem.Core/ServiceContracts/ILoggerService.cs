namespace BankingSystem.Core.ServiceContracts;

public interface ILoggerService
{
    void LogErrorInConsole(string errorMessage);
    void LogSuccessInConsole(string message);
}