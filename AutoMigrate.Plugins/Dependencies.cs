using AutoMigrate.Plugins.Configurations;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace AutoMigrate.Plugins;

public static class Dependencies
{
    public static void RegisterPlugins(this IKernelBuilderPlugins plugins)
    {
        plugins.AddFromType<SqlDatabasePlugins>();
        plugins.AddFromType<MigrationRulesPlugin>();
    }

    public static void RegisterPluginRequiredServices(this IServiceCollection services)
    {
        services.AddOptions<DatabaseConfiguration>()
            .BindConfiguration(DatabaseConfiguration.Section)
            .ValidateOnStart();

        services.AddOptions<BlobStorageConfiguration>()
            .BindConfiguration(BlobStorageConfiguration.Section)
            .ValidateOnStart();

        services.AddAzureClients(clientBuilder =>
        {
            var blobStorageConfiguration = services.BuildServiceProvider().GetRequiredService<IOptions<BlobStorageConfiguration>>().Value;

            clientBuilder.UseCredential(new AzureCliCredential());
            clientBuilder.AddBlobServiceClient(blobStorageConfiguration.ConnectionString);
        });

    }
}
