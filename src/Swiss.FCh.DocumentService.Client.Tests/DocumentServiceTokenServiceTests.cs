using System.Net;
using System.Net.Http.Json;
using Swiss.FCh.DocumentService.Client.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute.ClearExtensions;
using RichardSzalay.MockHttp;

namespace Swiss.FCh.DocumentService.Client.Tests;

[TestFixture]
internal sealed class DocumentServiceTokenServiceTests
{
    private DocumentServiceTokenService _documentServiceTokenService = null!;

    private readonly IHttpClientFactory _httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
    private readonly IOptions<DocumentServiceOptions> _documentServiceOptionsMock = Substitute.For<IOptions<DocumentServiceOptions>>();

    private readonly DocumentServiceOptions _documentServiceOptions = new()
    {
        Url = "url",
        TokenUrl = "http://token_url/",
        ClientId = "client_id",
        ClientSecret = "client_secret",
        RequestTimeoutMs = 1337
    };

    [SetUp]
    public void SetUp()
    {
        _documentServiceOptionsMock.Value.Returns(_documentServiceOptions);

        _documentServiceTokenService = new DocumentServiceTokenService(_httpClientFactoryMock, _documentServiceOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClientFactoryMock.ClearSubstitute();
        _documentServiceOptionsMock.ClearSubstitute();
    }

    [Test]
    public async Task GetToken_ShouldCallEndpoint()
    {
        var responseMock = new { access_token = "some_token", expires_in = 86400 };

        var httpMock = new MockHttpMessageHandler();
        httpMock.When(_documentServiceOptions.TokenUrl).Respond(HttpStatusCode.OK, JsonContent.Create(responseMock));
        _httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpMock.ToHttpClient());

        var token = await _documentServiceTokenService.GetToken().ConfigureAwait(false);

        _httpClientFactoryMock.Received(1).CreateClient(Arg.Any<string>());
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.EqualTo(responseMock.access_token));
    }

    [Test]
    public async Task GetToken_CalledMultipleTimes_ShouldReturnFromCacheAfterApiCall()
    {
        var responseMock = new { access_token = "some_token", expires_in = 86400 };

        var httpMock = new MockHttpMessageHandler();
        httpMock.When(_documentServiceOptions.TokenUrl).Respond(HttpStatusCode.OK, JsonContent.Create(responseMock));
        _httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(httpMock.ToHttpClient());

        await _documentServiceTokenService.GetToken().ConfigureAwait(false);
        await _documentServiceTokenService.GetToken().ConfigureAwait(false);
        await _documentServiceTokenService.GetToken().ConfigureAwait(false);
        await _documentServiceTokenService.GetToken().ConfigureAwait(false);
        var token = await _documentServiceTokenService.GetToken().ConfigureAwait(false);

        _httpClientFactoryMock.Received(1).CreateClient(Arg.Any<string>());
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.EqualTo(responseMock.access_token));
    }
}
