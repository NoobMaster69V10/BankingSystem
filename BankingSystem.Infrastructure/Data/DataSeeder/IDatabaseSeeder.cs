namespace BankingSystem.Infrastructure.Data.DataSeeder;

public interface IDatabaseSeeder
{
    Task SeedDataAsync();
}