using System.ClientModel;
using Azure.AI.OpenAI;
using Classification;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Set up DI
HostApplicationBuilder hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddUserSecrets<Program>();
hostBuilder.Services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Register an IChatClient

// GitHub Models

IChatClient innerChatClient = new AzureOpenAIClient(
    new Uri(hostBuilder.Configuration["AI:Endpoint"]!),
    new ApiKeyCredential(hostBuilder.Configuration["AI:Key"]!))
    .GetChatClient("gpt-4o-mini").AsIChatClient();

hostBuilder.Services.AddChatClient(innerChatClient);

// Run the app
var app = hostBuilder.Build();
var chatClient = app.Services.GetRequiredService<IChatClient>();

await Completion.RunAsync(chatClient);