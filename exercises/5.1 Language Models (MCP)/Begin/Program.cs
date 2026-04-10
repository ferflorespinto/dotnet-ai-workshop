using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using ModelContextProtocol.Client;

// Set up DI etc
var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration.AddUserSecrets<Program>();
hostBuilder.Services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register an IChatClient

// For GitHub Models or Azure OpenAI:
var innerChatClient = new AzureOpenAIClient(
    new Uri(hostBuilder.Configuration["AI:Endpoint"]!),
    new ApiKeyCredential(hostBuilder.Configuration["AI:Key"]!))
    .GetChatClient("gpt-4o-mini").AsIChatClient();

// Or for OpenAI Platform:
// var innerChatClient = new OpenAI.Chat.ChatClient("gpt-4o-mini", hostBuilder.Configuration["AI:Key"]!).AsIChatClient();

// Or for Ollama:
// var innerChatClient = new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.1");

hostBuilder.Services.AddChatClient(innerChatClient)
    .UseLanguage("Icelandic")
    .UseRateLimit(TimeSpan.FromSeconds(5))
    .UseFunctionInvocation();

// Run the app
var app = hostBuilder.Build();
var chatClient = app.Services.GetRequiredService<IChatClient>();

// TODO: Add your code here
List<ChatMessage> messages = [new(ChatRole.System, """
You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
""")];

// Create an MCP client that connects to the external server
IMcpClient mcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new()
    {
        Command = "docker",
        Arguments = ["run", "-i", "--rm", "mcpserver"],
        Name = "E-commerce MCP Server"
    }));

// Get tools from the MCP server
IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

// Use the tools in your chat options
var chatOptions = new ChatOptions 
{ 
    Tools = [.. tools] 
};

while (true)
{
    // Get input
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("\nYou: ");
    var input = Console.ReadLine()!;
    messages.Add(new(ChatRole.User, input));

    // Get reply
    var response = await chatClient.GetResponseAsync(messages, chatOptions);
    messages.AddMessages(response);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Bot: {response.Text}");
}