using GraphHero.DelegatingHandlers;
using GraphHero.HostedServices;
using GraphHero.Options;
using GraphHero.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        configHost.SetBasePath(currentDirectory);
        configHost.AddJsonFile("hostsettings.json", optional: false);
        configHost.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddLogging(configure => configure.AddConsole());

        services.AddSingleton<IValidateOptions<AzureAdOptions>, AzureAdOptionsValidation>();
        services.AddOptions();
        services.AddOptions<AzureAdOptions>()
            .Bind(configuration.GetSection("AzureAd"))
            .ValidateOnStart();

        services.AddSingleton<IGraphServiceClientProvider, GraphServiceClientProvider>();
        services.AddSingleton<AuditDelegatingHandler, AuditDelegatingHandler>();
        services.AddSingleton<ClientCredentialsKiotaAccessTokenProvider, ClientCredentialsKiotaAccessTokenProvider>();

        services.AddHostedService<AuthenticationHostedService>();
        //services.AddHostedService<CustomHeadersHostedService>();
        //services.AddHostedService<ErrorHandlingHostedService>();
        //services.AddHostedService<CustomDelegatingHandlerHostedService>();
        //services.AddHostedService<BatchRequestHostedService>();
        //services.AddHostedService<BetaEndpointHostedService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<IHost>>();
try
{
    host.Run();
}
catch (OptionsValidationException ex)
{
    foreach (var failure in ex.Failures)
    {
        logger!.LogError(failure);
    }
}
