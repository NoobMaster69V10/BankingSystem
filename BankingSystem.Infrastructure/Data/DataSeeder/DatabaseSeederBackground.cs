using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankingSystem.Infrastructure.Data.DataSeeder;

public class DatabaseSeederBackground : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceScopeFactory _scopeFactory;
    public DatabaseSeederBackground(IServiceScopeFactory scopeFactory, IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _scopeFactory = scopeFactory;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await scopedService.SeedDataAsync();
        _hostApplicationLifetime.ApplicationStarted.Register(() => { });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}