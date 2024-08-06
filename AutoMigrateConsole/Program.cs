using AutoMigrate.Console.Configurations;
using AutoMigrate.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder();

// Create configuration
IConfigurationRoot config = new ConfigurationBuilder()
                                .AddUserSecrets<Program>()
                                .Build();

// Register services
builder.Services.TryAddSingleton<IConfiguration>(config);
builder.Services.AddOptions<AzureOpenAiConfiguration>().BindConfiguration(AzureOpenAiConfiguration.Section).ValidateOnStart();

builder.Services.RegisterPluginRequiredServices();

// register the Azure OpenAI chat completion service
var azureOpenAiConfig = config.GetSection(AzureOpenAiConfiguration.Section).Get<AzureOpenAiConfiguration>()!;
builder.AddAzureOpenAIChatCompletion(azureOpenAiConfig.ModelId, azureOpenAiConfig.Endpoint, azureOpenAiConfig.ApiKey);

// Register plugins
builder.Plugins.RegisterPlugins();

// build the kernel
Kernel kernel = builder.Build();

// Retrieve the chat completion service
var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0,
};

var systemMessage = @"You are a helpful migration assistant tools with access to specific databases,
    - Databases are not connected so it will always be a 2 step procedure: get the data , set the data
    - Migration rules should content 2 sections: 1.Content and 2.Database structure
    - Migration rules content in natural language describing the migration.
    - Migration Rules database structure information grouped by database and describing all source tables and all destination tables, also include the following example per table : CREATE TABLE Orders (\r\n    OrderID int NOT NULL,\r\n    OrderNumber int NOT NULL,\r\n    PersonID int,\r\n    PRIMARY KEY (OrderID),\r\n    FOREIGN KEY (PersonID) REFERENCES Persons(PersonID)\r\n);
    - Migration Rules should always be saved based on the destination database, Filename should look like the following : {Destination Database Name}/{Destination TableName}/{source tables}.txt, Always replace space with _ BUT ONLY FOR FILE NAMES NOT OF THE CONTENT
    - If a user asks to validate something, execute the necessary calls/functions to check feasibility.
    - If a user asks to execute a migration, create and execute the query/script based on the available migration rules. If no rule is available, ask the user for more information.
    - Only Mention migration failures if it keeps failing after a retry";

// Create chat history
var history = new ChatHistory(systemMessage);
// Start the conversation
while (true)
{
    // Get user input
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("User > ");
    history.AddUserMessage(Console.ReadLine()!);

    // Get the response from the AI
    IAsyncEnumerable<StreamingChatMessageContent> result = chatCompletionService.GetStreamingChatMessageContentsAsync(history, kernel: kernel, executionSettings: openAIPromptExecutionSettings);

    // Stream the results
    string fullMessage = "";
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Assistant > ");
    await foreach (var content in result)
    {
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
    Console.WriteLine();

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(fullMessage);
}