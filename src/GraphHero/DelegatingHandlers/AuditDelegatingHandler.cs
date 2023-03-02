using System.Net;
using Microsoft.Graph.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GraphHero.DelegatingHandlers;

public class AuditDelegatingHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    public AuditDelegatingHandler(ILogger<AuditDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Requested URL: {request.RequestUri.AbsolutePath}");

        return base.SendAsync(request, cancellationToken);
    }
}