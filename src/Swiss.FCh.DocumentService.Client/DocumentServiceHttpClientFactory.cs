using System.Net.Http.Headers;
using Swiss.FCh.DocumentService.Client.Configuration;
using Microsoft.Extensions.Options;

namespace Swiss.FCh.DocumentService.Client;

internal class DocumentServiceHttpClientFactory : IDocumentServiceHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDocumentServiceTokenService _tokenService;
    private readonly DocumentServiceOptions _options;

    public DocumentServiceHttpClientFactory(IHttpClientFactory httpClientFactory, IDocumentServiceTokenService tokenService, IOptions<DocumentServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _options = options.Value;
    }

    public async Task<HttpClient> Create()
    {
        var client = _httpClientFactory.CreateClient(DocumentServiceOptions.HttpClientName);

        client.BaseAddress = new Uri(_options.Url);

        var token = await _tokenService.GetToken().ConfigureAwait(false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        client.Timeout = TimeSpan.FromMilliseconds(_options.RequestTimeoutMs);

        return client;
    }
}
