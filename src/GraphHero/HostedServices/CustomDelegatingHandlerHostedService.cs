using Azure.Identity;
using GraphHero.DelegatingHandlers;
using GraphHero.Options;
using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace GraphHero.HostedServices;

public class CustomDelegatingHandlerHostedService : IHostedService
{
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;
    private readonly ILogger _logger;
    private readonly AuditDelegatingHandler _handler;
    private readonly AzureAdOptions _azureAdOptions;
    private readonly ClientCredentialsKiotaAccessTokenProvider _authProvider;

    public CustomDelegatingHandlerHostedService(
        ILogger<BetaEndpointHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider,
        AuditDelegatingHandler handler,
        IOptions<AzureAdOptions> azureAdOptions,
        ClientCredentialsKiotaAccessTokenProvider authProvider)
    {
        _logger = logger;
        _graphServiceClientProvider = graphServiceClientProvider;
        _handler = handler;
        _azureAdOptions = azureAdOptions.Value;
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

        var usersWithNameStartingByCa = await graphServiceClient.Users.GetAsync(requestConfiguration => {
            requestConfiguration.QueryParameters.Filter = "startsWith('ca', displayName)";
            requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName"};
            requestConfiguration.QueryParameters.Count = true;
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

public class ClientCredentialsKiotaAccessTokenProvider : IAccessTokenProvider
{
    private readonly AzureAdOptions _azureAdOptions;
    public ClientCredentialsKiotaAccessTokenProvider(IOptions<AzureAdOptions> options)
    {
        _azureAdOptions = options.Value;
    }
    public AllowedHostsValidator AllowedHostsValidator => throw new NotImplementedException();

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var clientSecretCredential = new ClientSecretCredential(
            _azureAdOptions.TenantId, _azureAdOptions.ClientId, _azureAdOptions.ClientSecret);

        var accessToken = await clientSecretCredential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes), cancellationToken);
        return accessToken.Token;
    }
}