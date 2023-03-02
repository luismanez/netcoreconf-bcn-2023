using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using GraphHero.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace GraphHero.Providers;

public interface IGraphServiceClientProvider
{
    GraphServiceClient GetGraphServiceClientWithClientCredentialsAuth();
    GraphServiceClient GetGraphServiceClientWithDeviceCodeAuth();
    GraphServiceClient GetGraphServiceClientWithInteractiveProviderAuth();
    Microsoft.Graph.Beta.GraphServiceClient GetGraphBetaServiceClientWithClientCredentialsAuth();
}

public class GraphServiceClientProvider : IGraphServiceClientProvider
{
    private readonly AzureAdOptions _azureAdOptions;
    private readonly ILogger _logger;
    public GraphServiceClientProvider(
        IOptions<AzureAdOptions> azureAdOptions,
        ILogger<GraphServiceClientProvider> logger)
    {
        _azureAdOptions = azureAdOptions.Value;
        _logger = logger;
    }

    public GraphServiceClient GetGraphServiceClientWithClientCredentialsAuth()
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

    public GraphServiceClient GetGraphServiceClientWithDeviceCodeAuth()
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

    public GraphServiceClient GetGraphServiceClientWithInteractiveProviderAuth()
    {
        var scopes = new[] { "User.Read.All" };

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

    public Microsoft.Graph.Beta.GraphServiceClient GetGraphBetaServiceClientWithClientCredentialsAuth()
    {
        // The client credentials flow requires that you request the
        // /.default scope, and preconfigure your permissions on the
        // app registration in Azure. An administrator must grant consent
        // to those permissions beforehand.
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
        var clientSecretCredential = new ClientSecretCredential(
            _azureAdOptions.TenantId, _azureAdOptions.ClientId, _azureAdOptions.ClientSecret);

        return new Microsoft.Graph.Beta.GraphServiceClient(clientSecretCredential, scopes);
    }
}