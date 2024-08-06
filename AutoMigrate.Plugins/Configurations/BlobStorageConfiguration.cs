using System.ComponentModel.DataAnnotations;

namespace AutoMigrate.Plugins.Configurations;
public sealed record BlobStorageConfiguration
{
    public const string Section = "BlobStorageConfiguration";

    [Required]
    public required string ConnectionString { get; init; }
}
