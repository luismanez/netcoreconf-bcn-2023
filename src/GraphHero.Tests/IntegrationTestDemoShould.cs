using Moq;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Azure.Identity;

namespace GraphHero.Tests;

public class IntegrationTestDemoShould
{
    private readonly IConfiguration _configuration;
    public IntegrationTestDemoShould()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }
    [Fact]
    public async Task GetAryaStarkDisplayNameWhenCallingMeWithPasswordGrant()
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var azureAdSection = _configuration.GetSection("AzureAd");
        var user = azureAdSection.GetValue<string>("UserName");
        var password = azureAdSection.GetValue<string>("UserPassword");
        var tenantId = azureAdSection.GetValue<string>("TenantId");
        var clientId = azureAdSection.GetValue<string>("ClientId");

        var userNamePasswordCredential = new UsernamePasswordCredential(
            user,
            password,
            tenantId,
            clientId);

        var graphServiceClient = new GraphServiceClient(userNamePasswordCredential,
            scopes);

        var me = await graphServiceClient.Me.GetAsync(
            r => r.QueryParameters.Select = new string[] {"id", "displayName"});

        Assert.Equal("Arya Stark", me!.DisplayName);
    }
}