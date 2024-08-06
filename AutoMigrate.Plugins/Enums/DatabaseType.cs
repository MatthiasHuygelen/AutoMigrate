namespace AutoMigrate.Plugins.Enums;
public static class DatabaseType
{
    public const string SqlServer = "SqlServer";
    public const string MySql = "MySql";
    public const string Postgres = "Postgres";
    public const string Oracle = "Oracle";
    public const string Sqlite = "Sqlite";

    public static string[] SqlDatabases = { SqlServer, MySql, Postgres, Oracle, Sqlite };
}

