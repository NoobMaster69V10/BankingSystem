namespace BankingSystem.Infrastructure.Data.DatabaseConfiguration;

internal interface IDatabaseConfiguration
{
    Task ConfigureDatabaseAsync();
}