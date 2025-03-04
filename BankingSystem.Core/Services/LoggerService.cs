using Serilog;
using BankingSystem.Core.ServiceContracts;
using Microsoft.Extensions.Logging;

namespace BankingSystem.Core.Services;

public class LoggerService : ILoggerService
{
    private readonly Serilog.ILogger _fileLogger;
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
        _fileLogger = new LoggerConfiguration()
            .WriteTo.File("Logs/banking_system.log", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
    }
    public void LogError(string errorMessage)
    {
        _logger.LogError(errorMessage);
        _fileLogger.Error(errorMessage);
    }

    public void LogSuccess(string message)
    {
        _logger.LogInformation(message);
        _fileLogger.Information(message);
    }
}