using System.ComponentModel.DataAnnotations;

namespace AutoMigrate.Console.Configurations;

public sealed record AzureOpenAiConfiguration
{
    public const string Section = "AzureOpenAi";

    [Required]
    public required string ModelId { get; init; }
    [Required]
    public required string Endpoint { get; init; }
    [Required]
    public required string ApiKey { get; init; }
}
