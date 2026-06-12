using Swiss.FCh.DocumentService.Client.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute.ClearExtensions;
using RichardSzalay.MockHttp;

namespace Swiss.FCh.DocumentService.Client.Tests;

[TestFixture]
internal sealed class DocumentServiceHttpClientFactoryTests
{
    private DocumentServiceHttpClientFactory _documentServiceHttpClientFactory = null!;

    private readonly IHttpClientFactory _httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
    private readonly IDocumentServiceTokenService _documentServiceTokenService = Substitute.For<IDocumentServiceTokenService>();
    private readonly IOptions<DocumentServiceOptions> _documentServiceOptionsMock = Substitute.For<IOptions<DocumentServiceOptions>>();

    [SetUp]
    public void SetUp()
    {
        _documentServiceTokenService.GetToken().Returns("Test_Token");
        _documentServiceOptionsMock.Value.Returns(new DocumentServiceOptions
        {
            TokenUrl = "test_token_url",
            ClientId = "test_client_id",
            ClientSecret = "test_client_secret",
            Url = "http://test_url/",
            RequestTimeoutMs = 1337
        });

        _documentServiceHttpClientFactory = new DocumentServiceHttpClientFactory(_httpClientFactoryMock, _documentServiceTokenService, _documentServiceOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClientFactoryMock.ClearSubstitute();
        _documentServiceTokenService.ClearSubstitute();
        _documentServiceOptionsMock.ClearSubstitute();
    }

    [Test]
    public async Task Create_ShouldReturnClient()
    {
        using var httpMock = new MockHttpMessageHandler();
        _httpClientFactoryMock.CreateClient(DocumentServiceOptions.HttpClientName).Returns(httpMock.ToHttpClient());
        using var client = await _documentServiceHttpClientFactory.Create().ConfigureAwait(false);

        await _documentServiceTokenService.Received(1).GetToken().ConfigureAwait(false);
        Assert.That(client, Is.Not.Null);
        Assert.That(client.BaseAddress!.AbsoluteUri, Is.EqualTo(_documentServiceOptionsMock.Value.Url));
    }
}
