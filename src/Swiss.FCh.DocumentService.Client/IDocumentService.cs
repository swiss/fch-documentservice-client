using Swiss.FCh.DocumentService.Client.Models;

namespace Swiss.FCh.DocumentService.Client;

public interface IDocumentService
{
    Task<Stream> CreateExcel(Spreadsheet spreadsheet, SpreadsheetOptions? options = null);
    Task<Stream> CreateWordFromTemplate(string templateFilePath, object data, string? docRootElementName = null);
    Task<Stream> CreatePdfFromTemplate(string templateFilePath, object data, string? docRootElementName = null);
    Task<string> ExtractContentFromWord(string wordFilePath, WordExtractOptions options);
    Task<string> ExtractContentFromWord(Stream document, string fileName, WordExtractOptions options);
    Task<string> ConvertWordToHtml(string wordFilePath);
}
