using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Web;
using Swiss.FCh.DocumentService.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;

namespace Swiss.FCh.DocumentService.Client;

internal class DocumentService : IDocumentService
{
    private readonly IDocumentServiceHttpClientFactory _httpClientFactory;

    public DocumentService(IDocumentServiceHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Stream> CreateExcel(Spreadsheet spreadsheet, SpreadsheetOptions? options = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(spreadsheet);
            using var jsonStringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            if (options is not null)
            {
                if (options.SetAutoFilter)
                {
                    jsonStringContent.Headers.Add("setAutoFilter", options.SetAutoFilter.ToString());
                }
            }

            var stream = await GetResponse("api/v1/excel/generate", jsonStringContent).ConfigureAwait(false);
            return stream;
        }
        catch (Exception e)
        {
            throw new DocumentServiceException("Error occured while creating excel", e);
        }
    }

    public async Task<Stream> CreateWordFromTemplate(string templateFilePath, object data, string? docRootElementName = null)
    {
        try
        {
            return await CreateDocumentFromTemplate("template", templateFilePath, data, docRootElementName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new DocumentServiceException("Error occured while creating word from template", e);
        }
    }

    public async Task<Stream> CreatePdfFromTemplate(string templateFilePath, object data, string? docRootElementName = null)
    {
        try
        {
            return await CreateDocumentFromTemplate("pdf", templateFilePath, data, docRootElementName).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new DocumentServiceException("Error occured while creating pdf from template", e);
        }
    }

    public async Task<string> ExtractContentFromWord(string wordFilePath, WordExtractOptions options)
    {
        using var fileStream = File.OpenRead(wordFilePath);

        var fileName = Path.GetFileName(wordFilePath);
        return await ExtractContentFromWord(fileStream, fileName, options).ConfigureAwait(false);
    }

    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "MultipartFormDataContent will take ownership of HttpContent objects added to it and will dispose them."
    )]
    public async Task<string> ExtractContentFromWord(Stream document, string filename, WordExtractOptions options)
    {
        try
        {
            using var formData = new MultipartFormDataContent();

            document.Position = 0;
            var fileContent = new StreamContent(document);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);

            formData.Add(fileContent, "file", filename);
            formData.Headers.Add("Start-Paragraph", HttpUtility.UrlEncode(options.StartAfterParagraphText));
            if (!string.IsNullOrWhiteSpace(options.StopBeforeParagraphText))
            {
                formData.Headers.Add("End-Paragraph", HttpUtility.UrlEncode(options.StopBeforeParagraphText));
            }

            var uri = QueryHelpers.AddQueryString("api/v1/word/extraction", new Dictionary<string, string?>
            {
                { "exactStart", options.ExactStart.ToString() },
                { "exactEnd", options.ExactEnd.ToString() },
                { "strict", options.Strict.ToString() },
                { "format", options.Format.ToString() },
                { "lineBreaks", options.LineBreaks.ToString() },
                { "normalizerRemoveClasses", options.RemoveClasses.ToString() },
                { "normalizerRemoveStyles", options.RemoveStyles.ToString() },
                { "normalizerRemoveImages", options.RemoveImages.ToString() },
                { "normalizerRemoveEmptyAnchors", options.RemoveEmptyAnchors.ToString() },
                { "normalizerRemoveSpans", options.RemoveSpans.ToString() },
                { "normalizerReplaceItalicWithEmphasis", options.ReplaceItalicWithEmphasis.ToString() },
                { "normalizerReplaceBoldWithStrong", options.ReplaceBoldWithStrong.ToString() }
            });

            var stream = await GetResponse(uri, formData).ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync().ConfigureAwait(false);
            return result;
        }
        catch (Exception e)
        {
            throw new DocumentServiceException("Error occured while extracting content from document", e);
        }
    }

    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "MultipartFormDataContent will take ownership of HttpContent objects added to it and will dispose them."
    )]
    public async Task<string> ConvertWordToHtml(string wordFilePath)
    {
        try
        {
            using var formData = new MultipartFormDataContent();

            var fileStream = File.OpenRead(wordFilePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);

            formData.Add(fileContent, "file", Path.GetFileName(wordFilePath));

            var stream = await GetResponse("api/v1/word/conversion/html", formData).ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync().ConfigureAwait(false);
            return result;
        }
        catch (Exception e)
        {
            throw new DocumentServiceException("Error occured while converting word to html", e);
        }
    }

    [SuppressMessage(
        "Microsoft.Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "MultipartFormDataContent will take ownership of HttpContent objects added to it and will dispose them."
    )]
    private async Task<Stream> CreateDocumentFromTemplate(string route, string templateFilePath, object data, string? docRootElementName = null)
    {
        using var formData = new MultipartFormDataContent();
        var fileStream = File.OpenRead(templateFilePath);

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
        formData.Add(fileContent, "template", Path.GetFileName(templateFilePath));

        formData.Add(new StringContent(JsonSerializer.Serialize(data)), "jsonData");

        if (!string.IsNullOrWhiteSpace(docRootElementName))
        {
            formData.Add(new StringContent(docRootElementName), "docRootElementName");
        }

        var stream = await GetResponse($"api/v1/word/templating/{route}", formData).ConfigureAwait(false);
        return stream;
    }

    private async Task<Stream> GetResponse(string requestUri, HttpContent content)
    {
        using var client = await _httpClientFactory.Create().ConfigureAwait(false);
        var response = await client.PostAsync(requestUri, content).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var problemDetails =
                    await JsonSerializer.DeserializeAsync<ProblemDetails>(
                        await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);

                throw new DocumentServiceException($"Error while creating export: [{(int)response.StatusCode}] '{requestUri}' | Reason: '{problemDetails?.Title}'");
            }
            catch (Exception ex)
            {
                throw new DocumentServiceException($"Unexpected error from DocumentService: [{(int)response.StatusCode}] '{requestUri}'", ex);
            }
        }

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
}
