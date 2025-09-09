using VirtualAssistant.Api.Chats;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services
    .AddScoped<ChatService>()
    .AddSingleton(_ =>
    {
        var apiKeyCredential = new ApiKeyCredential("not-needed-for-local");

        var chatClient = new ChatClient("ai/gpt-oss", apiKeyCredential, new OpenAIClientOptions
        {
            Endpoint = new Uri("http://localhost:12434/engines/v1")
        });

        return chatClient;
    });

WebApplication app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/ai/chat/{conversationId:guid}", (
        [FromServices] ChatService chatService,
        [FromRoute] Guid conversationId,
        [FromBody] ChatRequest chatRequest,
        HttpContext httpContext) =>
    {
        httpContext.Response.Headers.CacheControl = "no-store";
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        return chatService.StreamDeltasAsync(conversationId, chatRequest);
    })
    .WithName("Chat");

app.MapDefaultEndpoints();

app.Run();
