namespace Swiss.FCh.DocumentService.Client;

public interface IDocumentServiceHttpClientFactory
{
    Task<HttpClient> Create();
}
