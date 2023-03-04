using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace GraphHero.HostedServices;

public class BatchRequestHostedService : IHostedService
{
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;
    private readonly ILogger _logger;

    public BatchRequestHostedService(
        ILogger<BatchRequestHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider)
    {
        _logger = logger;
        _graphServiceClientProvider = graphServiceClientProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var graphServiceClient = _graphServiceClientProvider
            .GetGraphServiceClientWithClientCredentialsAuth();

        var top3Users = graphServiceClient
                         .Users
                         .ToGetRequestInformation(
                            requestConfiguration =>
                                requestConfiguration.QueryParameters.Top = 3);

        var top3Groups = graphServiceClient
                         .Groups
                         .ToGetRequestInformation(
                            requestConfiguration =>
                                requestConfiguration.QueryParameters.Top = 3);

        // create Batch request container
        var batchRequestContent = new BatchRequestContent(graphServiceClient);

        // add batch steps
        var top3UsersRequestStepId = await batchRequestContent
            .AddBatchRequestStepAsync(top3Users);
        var top3GroupsRequestStepId = await batchRequestContent
            .AddBatchRequestStepAsync(top3Groups);

        // batch requests
        var batchResponseContent = await graphServiceClient.Batch.PostAsync(
            batchRequestContent, cancellationToken);

        var usersResponse = await batchResponseContent
            .GetResponseByIdAsync<UserCollectionResponse>(top3UsersRequestStepId);
        foreach (var user in usersResponse.Value!)
        {
            _logger.LogInformation(user.DisplayName);
        }

        var groupsResponse = await batchResponseContent
            .GetResponseByIdAsync<TeamCollectionResponse>(top3GroupsRequestStepId);
        foreach (var group in groupsResponse.Value!)
        {
            _logger.LogInformation(group.DisplayName);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("BatchRequestHostedService Stopped");
        return Task.CompletedTask;
    }
}