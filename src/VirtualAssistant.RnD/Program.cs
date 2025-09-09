using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using ChatMessage = OpenAI.Chat.ChatMessage;

// Sample CURL request for docker model runner engine

// curl http://localhost:12434/engines/v1/completions \
// -H "Content-Type: application/json" \
// -d '{
// "model": "ai/gpt-oss",
// "prompt": "Hello, who are you?",
// "max_tokens": 100
// }'

var apiKeyCredential = new ApiKeyCredential("not-needed-for-local");

var client = new ChatClient("ai/gpt-oss", apiKeyCredential, new OpenAIClientOptions
{
    Endpoint = new Uri("http://localhost:12434/engines/v1")
});

var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful assistant."),
    new UserChatMessage("Hello, who are you?")
};

AsyncCollectionResult<StreamingChatCompletionUpdate> response =
    client.CompleteChatStreamingAsync(messages, new ChatCompletionOptions
    {
        MaxOutputTokenCount = 100,
        Temperature = 0.7f,
        TopP = 0.95f
    });

await foreach (StreamingChatCompletionUpdate update in response.ConfigureAwait(false))
{
    if (update.ContentUpdate.Count > 0)
    {
        Console.Write(update.ContentUpdate[0].Text);
    }
}

/* Basic Chat Example */

/*
 * ClientResult<ChatCompletion> response =
 *     await client.CompleteChatAsync("Hello, who are you?").ConfigureAwait(false);
 *
 * Console.WriteLine(JsonSerializer.Serialize(response));
 */

Console.ReadLine();
