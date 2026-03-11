using System.ComponentModel.DataAnnotations;

namespace Swiss.FCh.DocumentService.Client.Configuration;

public class DocumentServiceOptions
{
    public const string SectionKey = "DocumentService";
    public const string HttpClientName = "Bk.DocumentService.Client";

    [Required]
    public required string Url { get; init; }

    [Required]
    public required string TokenUrl { get; init; }

    [Required]
    public required string ClientId { get; init; }

    [Required]
    public required string ClientSecret { get; init; }

    [Required]
    public int RequestTimeoutMs { get; init; }
}
