using BankingSystem.Core.ServiceContracts;
using Microsoft.Extensions.Logging;

namespace BankingSystem.Core.Services;

public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }
    public void LogError(string errorMessage)
    {
        _logger.LogError(errorMessage);
    }

    public void LogSuccess(string message)
    {
        _logger.LogInformation(message);
    }
}