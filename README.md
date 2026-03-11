# Introduction

This library simplyfies the usage of the *DocumentService* which is run by the federal chancellery.

## usage
### DI Injection
First, register the service in you DI container.
```
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDocumentService(builder.Configuration);
```

### Health Check
Add a health check for the document service.
```
builder.Services
       .AddHealthChecks()
       .AddDocumentService();
```

### Configuration
In your appsettings, add the following.

```json
{
    "DocumentService": {
	    "Url": "ds_service_url",
		"TokenUrl": "http://might_be_your_key_cloak/realms/bk-micro-services/protocol/openid-connect/token",
		"ClientId": "your_client_id",
		"ClientSecret": "your_client_secret",
		"RequestTimeoutMs": your_timeout
	}
}
```

### Excel Generation
Generate an Excel sheet like this.

```
		var spreadsheet = new Spreadsheet
        {
            HeaderCells = [...]
            BodyCells = [...],
            TableOptions = new TableOptions
            {
                CreateTable = true,
                ShowTotalsRow = true
            }
        };

        var resultStream = await _documentService.CreateExcel(spreadsheet);
```

### Template Generation
Generate a document based on an Aspose LINQ Syntax Template.

```
//generate a Word document
var resultingWordDoc = await _documentService.CreateWordFromTemplate($"pathToYourTemplate.docx", dataObject, "yourDataObjectRootElementName");

//generate a PDF
var resultingPdf = await _documentService.CreateWordFromTemplate($"pathToYourTemplate.docx", dataObject, "yourDataObjectRootElementName");
```

Note that your dataObject can be any object (it will be serialized to JSON and should match the placeholders in your word template).

### Word to HTML Conversion
You can convert a Word document to HTML as follows.

```
var result = await _documentService.ConvertWordToHtml("your_word_file_path");
```

### Extract Parts of a Document
You can extract parts of a Word document as follows.

```
var result = await _documentService.ExtractContentFromWord("your_word_file_path", new WordExtractOptions { .. });
```

The ```WordExtractOptions``` allow to configure the following.

---------------------------------------------------------------------------------------------------------------
|Property                  | Description                                                                      |
|StartAfterParagraphText   | Start the extraction after this text has occured (paragraph "starts with").      |
|StopBeforeParagraphText   | Stop the extraction before this text has occured (paragraph "starts with)".      |
|ExactStart                | Start the extraction after this text has occured (paragraph must match exaclty). |
|ExactEnd                  | Stop the extraction before this text has occured (paragraph must match exactly). |
|WordExtractFormat         | Extract result as HTML or PDF                                                    |
|LineBreaks                | Allow soft linebreaks in the search pattern                                      |
|RemoveClasses             | Removes all CSS classes from the resulting HTML                                  |
|RemoveStyles              | Removes all styles from the resulting HTML                                       |
|RemoveImages              | Removes images from the result                                                   |
|RemoveEmptyAnchors        | Removes all ```<a>``` tags with no ```href```                                    |
|RemoveSpans               | Removes all ```<span>``` but keeps their content                                 |
|ReplaceItalicWithEmphasis | Replaces ```<i>``` with ```<em>``` in the result HTML                            |
|ReplaceBoldWithStrong     | Replaces ```<b>``` with ```<strong>``` in the result HTML                        |

# Contribution
See: https://github.com/swiss/fch-documentservice-client/blob/main/CONTRIBUTING.md

# Security
See: https://github.com/swiss/fch-documentservice-client/blob/main/SECURITY.md

# Development Workflow

To publish a new version of the NuGet package, proceed as follows.

* apply and push your changes
* define and describe the new version in ```CHANGELOG.md```
* push the corresponding label with ```git tag vx.x.x``` and ```git push origin vx.x.x```
* go to GitHub -> Actions -> 'Build and Publish to NuGet.org' and trigger a run while specifying the correct GIT label
