using GraphHero.DelegatingHandlers;
using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace GraphHero.HostedServices;

public class CustomDelegatingHandlerHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly AuditDelegatingHandler _handler;
    private readonly ClientCredentialsKiotaAccessTokenProvider _authProvider;

    public CustomDelegatingHandlerHostedService(
        ILogger<CustomDelegatingHandlerHostedService> logger,
        AuditDelegatingHandler handler,
        ClientCredentialsKiotaAccessTokenProvider authProvider)
    {
        _logger = logger;
        _handler = handler;
        _authProvider = authProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var defaultHandlers = GraphClientFactory.CreateDefaultHandlers();
        defaultHandlers.Add(_handler);

        foreach (var handler in defaultHandlers)
        {
            _logger.LogInformation(handler.ToString());
        }

        var httpClient = GraphClientFactory.Create(defaultHandlers);

        var graphServiceClient = new GraphServiceClient(
            httpClient,
            new BaseBearerTokenAuthenticationProvider(_authProvider));

        var usersWithNameStartingByCa = await graphServiceClient
            .Users.GetAsync(requestConfiguration => {
                requestConfiguration.QueryParameters.Filter =
                    "startsWith('ca', displayName)";
                requestConfiguration.QueryParameters.Select =
                    new string[] { "id", "displayName"};
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
            }, cancellationToken);

        foreach (var user in usersWithNameStartingByCa!.Value!)
        {
            _logger.LogInformation(user.DisplayName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("CustomDelegatingHandlerHostedService Stopped");
        return Task.CompletedTask;
    }
}
