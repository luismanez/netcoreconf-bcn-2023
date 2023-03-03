using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;

namespace GraphHero.HostedServices;

public class ErrorHandlingHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;

    public ErrorHandlingHostedService(
        ILogger<ErrorHandlingHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider)
    {
        _logger = logger;
        _graphServiceClientProvider = graphServiceClientProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AuthenticationHostedService started...");

        var graphServiceClient = _graphServiceClientProvider.GetGraphServiceClientWithClientCredentialsAuth();

        try
        {
            var badRequest = await graphServiceClient.Users.GetAsync(
                requestConfiguration =>
                    requestConfiguration.QueryParameters.Filter="bad request",
                cancellationToken);
        }
        catch (ODataError odataError)
        {
            var mainErrorDetails = "no details";
            var mainErrorMessage = "no error message";
            if (odataError.Error != null)
            {
                mainErrorMessage = odataError.Error.Message;
                if (odataError.Error.Details != null)
                    mainErrorDetails = string.Join(",", odataError.Error.Details.Select(d => d.Message));
            }
            _logger.LogError(@$"Msg: {odataError.Message}. 
                StatusCode: {odataError.ResponseStatusCode}. 
                MainErrorMsg: {mainErrorMessage}. 
                MainErrorDetails: {mainErrorDetails}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("ErrorHandlingHostedService Stopped");
        return Task.CompletedTask;
    }
}