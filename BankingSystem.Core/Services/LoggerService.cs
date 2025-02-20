using BankingSystem.Core.ServiceContracts;
using Microsoft.Extensions.Logging;

namespace BankingSystem.Core.Services;

public class LoggerService(ILogger<LoggerService> logger) : ILoggerService
{
    public void LogErrorInConsole(string errorMessage)
    {
        logger.LogError(errorMessage);
    }

    public void LogSuccessInConsole(string message)
    {
        logger.LogInformation(message);
    }
}