using System.Net;
using Microsoft.Graph.Models;
using System.Text.Json;

namespace GraphHero.DelegatingHandlers;

public class MeInterceptorDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request!.RequestUri!.AbsolutePath.Contains("/me/"))
        {
            var meSimulatedUser = new User {
                Id = "",
                DisplayName = "Peter Parker",
                JobTitle = "Spiderman",
                UserPrincipalName = "pparker@dailybugle.com"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(meSimulatedUser))
            };

            return Task.FromResult(response);
        }

        return base.SendAsync(request, cancellationToken);
    }
}