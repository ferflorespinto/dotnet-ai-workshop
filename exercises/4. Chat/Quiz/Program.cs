using QuizApp.Components;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using System.ClientModel;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// TODO:
//  - Decide which LLM backend you're going to use, and register it in DI
//  - It can be any IChatClient implementation, for example AzureOpenAIClient or OllamaChatClient
//  - See instructions for sample code
AzureOpenAIClient azureClient = new AzureOpenAIClient(
    new Uri(builder.Configuration["AI:Endpoint"]!),
    new ApiKeyCredential(builder.Configuration["AI:Key"]!));

ChatClient chatClient = azureClient.GetChatClient("gpt-4o-mini");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddChatClient(chatClient.AsIChatClient());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
