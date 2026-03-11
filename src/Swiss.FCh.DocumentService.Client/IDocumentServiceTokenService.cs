namespace Swiss.FCh.DocumentService.Client;

public interface IDocumentServiceTokenService
{
    Task<string> GetToken();
}
