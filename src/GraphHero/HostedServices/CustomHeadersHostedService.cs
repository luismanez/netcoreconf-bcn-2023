using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphHero.Providers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraphHero.HostedServices;

public class CustomHeadersHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IGraphServiceClientProvider _graphServiceClientProvider;

    public CustomHeadersHostedService(
        ILogger<CustomHeadersHostedService> logger,
        IGraphServiceClientProvider graphServiceClientProvider)
    {
        _graphServiceClientProvider = graphServiceClientProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string userId = "2e7aa741-f341-406a-9d55-e39f15c4c645";
        const string groupId = "85f38d38-3d10-4154-9b24-45451eb3dfdd";
        const string anotherGroupId = "4ddc56ea-bdcf-4a53-9fb3-3b8d5631dd17";
        var unexistentGroupId = Guid.NewGuid().ToString();

        var graphServiceClient = _graphServiceClientProvider.GetGraphServiceClientWithClientCredentialsAuth();

        // Filtering with the in (xxx,xxx) requires ConsystencyLevel and Count
        var userGroupsMatchingPassedOnes = await graphServiceClient
            .Users[userId]
            .TransitiveMemberOf
            .GraphGroup // !!!!!!!!! SDK v5 supports OData Cast! before this, you got a DirectoryObject object
            .GetAsync(requestConfiguration => {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Filter = $"id in ('{groupId}','{anotherGroupId}','{unexistentGroupId}')";
                requestConfiguration.QueryParameters.Count = true;
            }, cancellationToken);

        // If your Query only wants the Count, the SDK v5 makes it easy:
        var userGroupsMatchingPassedOnesCount = await graphServiceClient
            .Users[userId].TransitiveMemberOf.Count.GetAsync(requestConfiguration => {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Filter = $"id in ('{groupId}','{anotherGroupId}','{unexistentGroupId}')";
            }, cancellationToken);

        foreach (var matchedGroup in userGroupsMatchingPassedOnes!.Value!)
        {
            _logger.LogInformation($"User is in group: {matchedGroup.DisplayName}");
        }

        _logger.LogInformation(
            $"User matches {userGroupsMatchingPassedOnesCount.GetValueOrDefault()} of the passed Groups");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("CustomHeadersHostedService Stopped");
        return Task.CompletedTask;
    }
}