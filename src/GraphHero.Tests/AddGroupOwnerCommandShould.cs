using Microsoft.Graph;
using Microsoft.Graph.Models;
using Moq;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Graph.Models.ODataErrors;

namespace GraphHero.Tests;

public class AddGroupOwnerCommandShould
{
    [Fact]
    public async Task AddOwnerToGroupWhenUsingIsInCompanyChampionGroup()
    {
        var mockRequestAdapter = new Mock<IRequestAdapter>();
        var graphServiceClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.Setup(
                        adapter => adapter.SendAsync(
                            It.IsAny<RequestInformation>(), User.CreateFromDiscriminatorValue,
                            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                            It.IsAny<CancellationToken>() )
                    ).ReturnsAsync(new User());

        var groupsCollectionResponse = new GroupCollectionResponse() {
                Value = new List<Group>
                {
                    new Group { DisplayName = "Mocked group"}
                }
        };

        mockRequestAdapter.Setup(
                        adapter => adapter.SendAsync(
                            It.IsAny<RequestInformation>(), GroupCollectionResponse.CreateFromDiscriminatorValue,
                            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                            It.IsAny<CancellationToken>())
                    ).ReturnsAsync(groupsCollectionResponse);

        mockRequestAdapter.Setup(
                adapter => adapter.SerializationWriterFactory.GetSerializationWriter(
                    It.IsAny<string>()))
            .Returns(new JsonSerializationWriter());

        mockRequestAdapter.Setup(
                adapter => adapter.SendNoContentAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddOwnerCommand(graphServiceClient);
        await command.AddGroupOwner("a user id", "a group id");
    }

    [Fact]
    public async Task ThrowUserIsNotCompanyChampionWhenUserIsNotMemberOfTheCompanyChampionsGroup()
    {
        var mockRequestAdapter = new Mock<IRequestAdapter>();
        var graphServiceClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.Setup(
                        adapter => adapter.SendAsync(
                            It.IsAny<RequestInformation>(), User.CreateFromDiscriminatorValue,
                            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                            It.IsAny<CancellationToken>() )
                    ).ReturnsAsync(new User());

        mockRequestAdapter.Setup(
                        adapter => adapter.SendAsync(
                            It.IsAny<RequestInformation>(),
                            GroupCollectionResponse.CreateFromDiscriminatorValue,
                            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ODataError() { ResponseStatusCode = 404});

        mockRequestAdapter.Setup(
                adapter => adapter.SerializationWriterFactory.GetSerializationWriter(
                    It.IsAny<string>()))
            .Returns(new JsonSerializationWriter());

        mockRequestAdapter.Setup(
                adapter => adapter.SendNoContentAsync(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddOwnerCommand(graphServiceClient);

        await Assert.ThrowsAsync<UserIsNotCompanyChampion>(
            async () => await command.AddGroupOwner("a user id", "a group id"));
    }
}