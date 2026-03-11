namespace Swiss.FCh.DocumentService.Client.Models;

public enum WordExtractFormat
{
    Html,
    PlainText
}

public class WordExtractOptions
{
#pragma warning disable CA1805 //explicit assignment of default values for better readability
    public required string StartAfterParagraphText { get; init; }
    public string? StopBeforeParagraphText { get; init; }
    public bool ExactStart { get; init; } = true;
    public bool ExactEnd { get; init; } = true;
    public bool Strict { get; init; } = false;
    public WordExtractFormat Format { get; init; } = WordExtractFormat.Html;
    public bool LineBreaks { get; init; } = true;
    public bool RemoveClasses { get; init; } = false;
    public bool RemoveStyles { get; init; } = false;
    public bool RemoveImages { get; init; } = false;
    public bool RemoveEmptyAnchors { get; init; } = false;
    public bool RemoveSpans { get; init; } = false;
    public bool ReplaceItalicWithEmphasis { get; init; } = false;
    public bool ReplaceBoldWithStrong { get; init; } = false;
#pragma warning restore CA1805
}
