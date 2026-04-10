using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

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
//var innerChatClient = new OllamaChatClient(new Uri("http://localhost:11434"), "smollm2:135m");

hostBuilder.Services.AddChatClient(innerChatClient)
    .UseFunctionInvocation();

// Run the app
var app = hostBuilder.Build();
var chatClient = app.Services.GetRequiredService<IChatClient>();

List<ChatMessage> messages = [new(ChatRole.System, """
You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
""")];

var cart = new Cart();
AIFunction getPriceTool = AIFunctionFactory.Create(cart.GetPrice);
AIFunction addToCartTool = AIFunctionFactory.Create(cart.AddSocksToCart);
AIFunction removeFromCartTool = AIFunctionFactory.Create(cart.RemoveSocksFromCart);
AIFunction clearCartTool = AIFunctionFactory.Create(cart.ClearCart);
AIFunction getCartTool = AIFunctionFactory.Create(cart.GetCart);
var chatOptions = new ChatOptions { Tools = [addToCartTool, getPriceTool, removeFromCartTool, clearCartTool, getCartTool] };

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

class Cart
{
    public int NumPairsOfSocks { get; set; }

    [Description("Adds the specified number of pairs of socks to the cart.")]
    public void AddSocksToCart(int numPairs)
    {
        NumPairsOfSocks += numPairs;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"Added {numPairs} pairs to your cart. Total: {NumPairsOfSocks} pairs.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Removes the specified number of pairs of socks to the cart.")]
    public void RemoveSocksFromCart(int numPairs)
    {
        if (NumPairsOfSocks - numPairs < 0)
        {
            Console.WriteLine("*****");
            Console.WriteLine($"Could not remove {numPairs} from your cart.");
            Console.WriteLine("*****");
            return;
        }
        NumPairsOfSocks -= numPairs;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"Removed {numPairs} pairs from your cart. Total: {NumPairsOfSocks} pairs.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Empties the cart contents and removes all pairs of socks.")]
    public void ClearCart()
    {
        NumPairsOfSocks = 0;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"Cleared all socks from your cart.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Gets the number of pairs of socks currently in the cart.")]
    public int GetCart() => NumPairsOfSocks;

    [Description("Computes the price of socks, returning a value in dollars.")]
    public float GetPrice(
        [Description("The number of pairs of socks to calculate price for")] int count) 
        => count * 15.99f;
}