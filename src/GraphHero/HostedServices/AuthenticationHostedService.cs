using Azure.Identity;
using GraphHero.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace GraphHero.HostedServices;

public class AuthenticationHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly AzureAdOptions _azureAdOptions;

    public AuthenticationHostedService(
        ILogger<AuthenticationHostedService> logger,
        IOptions<AzureAdOptions> azureAdOptions)
    {
        _logger = logger;
        _azureAdOptions = azureAdOptions.Value;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AuthenticationHostedService started...");

        //var graphServiceClient = GetGraphServiceClientWithClientCredentialsAuth();
        //var graphServiceClient = GetGraphServiceClientWithDeviceCodeAuth();
        var graphServiceClient = GetGraphServiceClientWithInteractiveProviderAuth();

        var top5Users = await graphServiceClient.Users.GetAsync(requestConfiguration => {
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

    // Returns a GraphServiceClient using ClientCredentials flow (Application permissions)
    private GraphServiceClient GetGraphServiceClientWithClientCredentialsAuth()
    {
        // The client credentials flow requires that you request the
        // /.default scope, and preconfigure your permissions on the
        // app registration in Azure. An administrator must grant consent
        // to those permissions beforehand.
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
        var clientSecretCredential = new ClientSecretCredential(
            _azureAdOptions.TenantId, _azureAdOptions.ClientId, _azureAdOptions.ClientSecret);

        return new GraphServiceClient(clientSecretCredential, scopes);
    }

    private GraphServiceClient GetGraphServiceClientWithDeviceCodeAuth()
     {
        var scopes = new[] { "User.Read.All" };

        // Callback function that receives the user prompt
        // Prompt contains the generated device code that use must
        // enter during the auth process in the browser
        static Task callback(DeviceCodeInfo code, CancellationToken cancellation)
        {
            Console.WriteLine(code.Message);
            return Task.FromResult(0);
        }

        var deviceCodeCredential = new DeviceCodeCredential(
            callback, _azureAdOptions.TenantId, _azureAdOptions.ClientId);

        return new GraphServiceClient(deviceCodeCredential, scopes);
     }

    private GraphServiceClient GetGraphServiceClientWithInteractiveProviderAuth()
    {
        var scopes = new[] { "User.Read.All" };

        // using Azure.Identity;
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = _azureAdOptions.TenantId,
            ClientId = _azureAdOptions.ClientId,
            // MUST be http://localhost or http://localhost:PORT
            // See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
            RedirectUri = new Uri("http://localhost"),
        };

        var interactiveCredential = new InteractiveBrowserCredential(options);

        return new GraphServiceClient(interactiveCredential, scopes);
    }
}