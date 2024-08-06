using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using Azure;
using System.Text.Json;

namespace AutoMigrate.Plugins;
public sealed class MigrationRulesPlugin
{
    private const string ContainerName = "rules";
    private readonly BlobServiceClient _blobServiceClient;

    public MigrationRulesPlugin(BlobServiceClient BlobServiceClient)
    {
        _blobServiceClient = BlobServiceClient;
    }

    [KernelFunction, Description("Delete migration rule")]
    public async Task Delete(
        [Description("Filename")] string fileName)
    {
        var blobServiceClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await blobServiceClient.DeleteBlobIfExistsAsync(fileName, DeleteSnapshotsOption.None, null);
    }

    [KernelFunction, Description("Save migration rule")]
    [return: Description("True if the file was saved successfully")]
    public async Task<string> Save(
        [Description("Filename")] string fileName, 
        [Description("Textual Content of the migration rule")] string content,
        [Description("User has accepted to overwrite rules")] bool overwrite)
    {
        var blobServiceClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = blobServiceClient.GetBlobClient(fileName);
        var doesBlobExistResult = await blobClient.ExistsAsync();

        if (doesBlobExistResult.Value && !overwrite)
        {
            return "File already exists. Validate if you want to overwrite it.";
        }

        try
        {
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), overwrite);
            return "true";
        }
        catch (Exception)
        {
            return "false";
        }
    }

    [KernelFunction, Description("Get a specific migration rule by filename")]
    public async Task<string> Get([Description("Filename")] string filename)
    {
        var blobServiceClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = blobServiceClient.GetBlobClient(filename);
        var response = await blobClient.DownloadAsync();
        var content = await new StreamReader(response.Value.Content).ReadToEndAsync();

        return content;
    }

    // TODO: add filters
    [KernelFunction, Description("Search all migration rules")]
    public async Task<string> Search()
    {
        var blobServiceClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var files = new List<string>();
        await foreach (Page<BlobItem> blobPage in blobServiceClient.GetBlobsAsync().AsPages())
        {
            files.AddRange(blobPage.Values.Select(b => b.Name));
        }
        return JsonSerializer.Serialize(files);
    }
}
