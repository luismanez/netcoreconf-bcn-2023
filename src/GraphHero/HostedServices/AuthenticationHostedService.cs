using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraphHero.HostedServices;

public class AuthenticationHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;

    public AuthenticationHostedService(
        ILogger<AuthenticationHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider)
    {
        _logger = logger;
        _graphServiceClientProvider = graphServiceClientProvider;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AuthenticationHostedService started...");

        // var graphServiceClient = _graphServiceClientProvider
        //     .GetGraphServiceClientWithClientCredentialsAuth();
        // var graphServiceClient = _graphServiceClientProvider
        //     .GetGraphServiceClientWithDeviceCodeAuth();
        var graphServiceClient = _graphServiceClientProvider
            .GetGraphServiceClientWithInteractiveProviderAuth();

        var top5Users = await graphServiceClient.Users.GetAsync(
            requestConfiguration => {
                requestConfiguration.QueryParameters.Select = new[] { "id", "displayName" };
                requestConfiguration.QueryParameters.Top = 5;
        }, cancellationToken);

        foreach (var user in top5Users!.Value!)
        {
            _logger.LogInformation(user.DisplayName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("AuthenticationHostedService Stopped");
        return Task.CompletedTask;
    }
}