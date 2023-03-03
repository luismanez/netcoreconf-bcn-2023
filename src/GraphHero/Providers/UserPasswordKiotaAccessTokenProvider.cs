using Azure.Core;
using Azure.Identity;
using GraphHero.Options;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace GraphHero.Providers;

public class UserPasswordKiotaAccessTokenProvider : IAccessTokenProvider
{
    private readonly AzureAdOptions _azureAdOptions;
    public UserPasswordKiotaAccessTokenProvider(IOptions<AzureAdOptions> options)
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

        var userNamePasswordCredential = new UsernamePasswordCredential(
            _azureAdOptions.UserName,
            _azureAdOptions.UserPassword,
            _azureAdOptions.TenantId,
            _azureAdOptions.ClientId);

        var accessToken = await userNamePasswordCredential.GetTokenAsync(
            new TokenRequestContext(scopes),
            cancellationToken);

        return accessToken.Token;
    }
}