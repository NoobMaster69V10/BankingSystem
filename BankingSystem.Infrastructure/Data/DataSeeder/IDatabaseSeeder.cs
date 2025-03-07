namespace BankingSystem.Infrastructure.Data.DataSeeder;

public interface IDatabaseSeeder
{
    public Task SeedDataAsync();
}