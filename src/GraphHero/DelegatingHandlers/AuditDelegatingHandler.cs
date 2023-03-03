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
        _logger.LogInformation($"**** AUDIT_HANDLER**** Requested URL: {request.RequestUri!.AbsoluteUri}");
        foreach (var header in request.Headers)
        {
            string headerName = header.Key;
            string headerContent = string.Join(",", header.Value.ToArray());
            _logger.LogInformation($"{headerName}={headerContent}");
        }

        return base.SendAsync(request, cancellationToken);
    }
}