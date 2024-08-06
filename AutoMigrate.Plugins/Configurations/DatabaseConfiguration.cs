namespace AutoMigrate.Plugins.Configurations;

public sealed record DatabaseConfiguration
{
    public const string Section = "DatabaseConfiguration";
    public IReadOnlyCollection<Database> Databases { get; init; } = [];
}

public sealed record Database{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Location { get; init; }
    public required string ConnectionString { get; init; }
}