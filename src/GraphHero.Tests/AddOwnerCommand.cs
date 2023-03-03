using System.Text.RegularExpressions;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace GraphHero.Tests;

public class AddOwnerCommand
{
    private readonly GraphServiceClient _graphServiceClient;
    public AddOwnerCommand(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task AddGroupOwner(string userId, string groupId)
    {
        try
        {
            const string companyChampionsGroupId = "4ddc56ea-bdcf-4a53-9fb3-3b8d5631dd17";

            var user = await _graphServiceClient.Users[userId].GetAsync();

            // check if the user is member of the CompanyChampions groups
            // otherwise, we throw an exception
            var companyChampionsGroup = await _graphServiceClient
                .Users[userId]
                .TransitiveMemberOf
                .GraphGroup
                .GetAsync(config => {
                    config.QueryParameters.Filter = $"id eq '{companyChampionsGroupId}'";
                    config.QueryParameters.Select = new [] {"id", "displayName"};
                    config.QueryParameters.Top = 1;
                });

            // add the user as Owner of the passed group
            var data = new ReferenceCreate() { OdataId = userId};
            await _graphServiceClient
                .Groups[groupId]
                .Owners.Ref.PostAsync(data, null);
        }
        catch (ODataError odataError)
        {
            if (odataError.ResponseStatusCode == 404)
                throw new UserIsNotCompanyChampion($"User {userId} is not member of ComapnyChampions group");

            throw;
        }
    }
}

public class UserIsNotCompanyChampion : Exception
{
    public UserIsNotCompanyChampion() : base()
    {
    }

    public UserIsNotCompanyChampion(string? message) : base(message)
    {
    }

    public UserIsNotCompanyChampion(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}