using Azure.Identity;
using GraphHero.Options;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace GraphHero.Providers;

public class ClientCredentialsKiotaAccessTokenProvider : IAccessTokenProvider
{
    private readonly AzureAdOptions _azureAdOptions;
    public ClientCredentialsKiotaAccessTokenProvider(IOptions<AzureAdOptions> options)
    {
        _azureAdOptions = options.Value;
    }
    public AllowedHostsValidator AllowedHostsValidator =>
        throw new NotImplementedException();

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var clientSecretCredential = new ClientSecretCredential(
            _azureAdOptions.TenantId,
            _azureAdOptions.ClientId,
            _azureAdOptions.ClientSecret);

        var accessToken = await clientSecretCredential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(scopes), cancellationToken);
        return accessToken.Token;
    }
}