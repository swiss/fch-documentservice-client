namespace Swiss.FCh.DocumentService.Client.Models;

public class SpreadsheetOptions
{
    public bool SetAutoFilter { get; init; }

    public static SpreadsheetOptions Default = new SpreadsheetOptions
    {
        SetAutoFilter = false
    };
}

public class Spreadsheet
{
    public IList<Cell> HeaderCells { get; init; } = [];
    public IList<IList<Cell>> BodyCells { get; init; } = [];
    public TableOptions? TableOptions { get; set; }
}

public class Cell
{
    public string? Text { get; init; }
    public string? Format { get; init; }
    public string? FormatType { get; init; }
    public string? ForegroundColor { get; init; }
    public string? BackgroundColor { get; init; }
    public string? HAlignment { get; init; }
    public string? VAlignment { get; init; }
    public string? Formula { get; init; }
    public bool IsFormula => !string.IsNullOrWhiteSpace(Formula);
    public string? Hyperlink { get; init; }
    public bool IsHyperlink => !string.IsNullOrWhiteSpace(Hyperlink);
    public bool SkipValue => string.IsNullOrWhiteSpace(Text) || IsFormula || IsHyperlink;
}

public static class CellFormat
{
    public const string Bold = "bold";
    public const string Italic = "italic";
    public const string Wrap = "wrap";
}

public static class CellFormatTypes
{
    public const string Number = "number";
    public const string Date = "date";
    public const string Percentage = "percentage";
}

public static class CellHAlignment
{
    public const string Left = "left";
    public const string Center = "center";
    public const string Right = "right";
}

public static class CellVAlignment
{
    public const string Top = "top";
    public const string Center = "center";
    public const string Bottom = "bottom";
}

public class TableOptions
{
    public bool CreateTable { get; init; }
    public string? TableRange { get; init; }
    public bool ShowTotalsRow { get; init; }
    public IList<TotalCell> TotalCells { get; init; } = [];
}

public class TotalCell
{
    public required string Field { get; init; }
    public string? Function { get; set; }
    public string? Label { get; set; }
    public string? Format { get; set; }
}

public static class TotalCellFunctions
{
    public const string Sum = "sum";
    public const string Average = "average";
    public const string Count = "count";
    public const string Max = "max";
    public const string Min = "min";
}
