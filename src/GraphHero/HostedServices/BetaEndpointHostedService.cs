using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraphHero.HostedServices;

public class BetaEndpointHostedService : IHostedService
{
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;
    private readonly ILogger _logger;

    public BetaEndpointHostedService(
        ILogger<BetaEndpointHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider)
    {
        _logger = logger;
        _graphServiceClientProvider = graphServiceClientProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string userId = "2e7aa741-f341-406a-9d55-e39f15c4c645";

        var betaClient = _graphServiceClientProvider
            .GetGraphBetaServiceClientWithClientCredentialsAuth();

        var userTop3Awards = await betaClient.Users[userId]
            .Profile.Awards.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "webUrl" };
                requestConfiguration.QueryParameters.Top = 3;
            }, cancellationToken);

        foreach (var award in userTop3Awards!.Value!)
        {
            _logger.LogInformation($"User {userId} was awarded as: {award.DisplayName}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("BetaEndpointHostedService Stopped");
        return Task.CompletedTask;
    }
}