using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankingSystem.Infrastructure.Data.DatabaseConfiguration;

public class DatabaseConfiguratorBackground : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public DatabaseConfiguratorBackground(IServiceScopeFactory scopeFactory, IHostApplicationLifetime hostApplicationLifetime)
    {
        _scopeFactory = scopeFactory;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IDatabaseConfiguration>();

        await scopedService.ConfigureDatabaseAsync();
        _hostApplicationLifetime.ApplicationStarted.Register(() => {});
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}