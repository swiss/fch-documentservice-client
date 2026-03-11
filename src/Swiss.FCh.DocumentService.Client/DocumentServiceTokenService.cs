using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Swiss.FCh.DocumentService.Client.Configuration;
using Microsoft.Extensions.Options;

namespace Swiss.FCh.DocumentService.Client;

internal class DocumentServiceTokenService : IDocumentServiceTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DocumentServiceOptions _documentServiceOptions;

    private string? _accessToken;
    private DateTime _expirationDate;

    public DocumentServiceTokenService(IHttpClientFactory httpClientFactory, IOptions<DocumentServiceOptions> documentServiceOptions)
    {
        ArgumentNullException.ThrowIfNull(documentServiceOptions);

        _httpClientFactory = httpClientFactory;
        _documentServiceOptions = documentServiceOptions.Value;
    }

    public async Task<string> GetToken()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _expirationDate > DateTime.UtcNow)
        {
            return _accessToken;
        }

        using var client = _httpClientFactory.CreateClient();
        var uri = new UriBuilder(_documentServiceOptions.TokenUrl);
        var body = new FormUrlEncodedContent([new KeyValuePair<string, string>("grant_type", "client_credentials")]);

        var bytes = new UTF8Encoding().GetBytes($"{_documentServiceOptions.ClientId}:{_documentServiceOptions.ClientSecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

        var response = await client.PostAsync(uri.Uri, body).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var token = JsonSerializer.Deserialize<DocumentServiceTokenResponse>(content);

        if (token is null)
        {
            _accessToken = null;
            throw new AuthenticationFailureException("Could not retrieve document service token");
        }

        _accessToken = token.AccessToken;
        _expirationDate = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 10);

        return _accessToken;
    }
}

internal class DocumentServiceTokenResponse
{
    [JsonPropertyName("access_token"), Required]
    public required string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public double ExpiresIn { get; init; }
}
