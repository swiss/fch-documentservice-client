using Swiss.FCh.DocumentService.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Swiss.FCh.DocumentService.Client.Extensions;

public static class DocumentServiceCollectionExtensions
{
    public static void AddDocumentService(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<DocumentServiceOptions>()
            .Bind(configuration.GetSection(DocumentServiceOptions.SectionKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDocumentServiceTokenService, DocumentServiceTokenService>();
        services.AddSingleton<IDocumentServiceHttpClientFactory, DocumentServiceHttpClientFactory>();

        services
            .AddHttpClient(DocumentServiceOptions.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false
            });

        services.AddScoped<IDocumentService, DocumentService>();
    }

    public static IHealthChecksBuilder AddDocumentService(this IHealthChecksBuilder builder, string name = "documentService")
    {
        return builder.AddCheck<DocumentServiceHealthCheck>(name);
    }
}
