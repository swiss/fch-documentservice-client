using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Swiss.FCh.DocumentService.Client;

public class DocumentServiceHealthCheck : IHealthCheck
{
    private readonly IDocumentServiceHttpClientFactory _documentServiceHttpClientFactory;

    public DocumentServiceHealthCheck(IDocumentServiceHttpClientFactory documentServiceHttpClientFactory, IDocumentService documentService)
    {
        _documentServiceHttpClientFactory = documentServiceHttpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            using var client = await _documentServiceHttpClientFactory.Create().ConfigureAwait(false);
            var response = await client.GetAsync("api/v1/systemstatus", cancellationToken).ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Document Service is reachable.")
                : context.Registration.FailureStatus == HealthStatus.Degraded
                    ? HealthCheckResult.Degraded($"Document Service returned status code ({(int)response.StatusCode}) {response.StatusCode}.")
                    : HealthCheckResult.Unhealthy($"Document Service returned status code ({(int)response.StatusCode}) {response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return context.Registration.FailureStatus == HealthStatus.Degraded
                ? HealthCheckResult.Degraded("Error connecting to Document Service.", ex)
                : HealthCheckResult.Unhealthy("Error connecting to Document Service.", ex);
        }
    }
}
