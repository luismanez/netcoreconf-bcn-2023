using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraphHero.HostedServices;

public class AuthenticationHostedService : IHostedService
{
    private readonly ILogger _logger;
    public AuthenticationHostedService(ILogger<AuthenticationHostedService> logger)
    {
        _logger = logger;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AuthenticationHostedService started...");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("AuthenticationHostedService Stopped");
        return Task.CompletedTask;
    }
}