using AutoMigrate.Plugins.Configurations;
using AutoMigrate.Plugins.Enums;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace AutoMigrate.Plugins;

public sealed class SqlDatabasePlugins
{
    private readonly DatabaseConfiguration _options;

    public SqlDatabasePlugins(IOptions<DatabaseConfiguration> options)
    {
        _options = options.Value;
    }

    [KernelFunction, Description("Execute query on a SQL database and return result")]
    public async Task<string> ExecuteQueryAsJsonAsync(
        [Description("Query to execute")] string query,
        [Description("Database key")] string databaseKey)
    {
        var database = _options.Databases.FirstOrDefault(x => x.Name == databaseKey);
        if (database is null)
        {
            return "Database not found";
        }

        if (!DatabaseType.SqlDatabases.Contains(database.Type))
        {
            return "Function doesn't support this database type";
        }

        await using var connection = new SqlConnection(database.ConnectionString);
        var result = await connection.QueryAsync(query);
        return JsonSerializer.Serialize(result);
    }

    [KernelFunction, Description("Get available SQL databases")]
    public string GetDatabases()
    {
        var databases = _options.Databases.Where(x => DatabaseType.SqlDatabases.Contains(x.Type))
                                          .Select(x => new { x.Name , x.Location } ).ToList();
        return JsonSerializer.Serialize(databases);
    }
}
