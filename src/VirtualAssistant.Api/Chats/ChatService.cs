using OpenAI.Chat;
using System.ClientModel;

namespace VirtualAssistant.Api.Chats;

public class ChatService(ChatClient chatClient)
{
    public async IAsyncEnumerable<ChatDelta> StreamDeltasAsync(Guid conversationId, ChatRequest request)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            // new UserChatMessage("Hello, who are you?")
            new UserChatMessage(request.Prompt)
        };

        AsyncCollectionResult<StreamingChatCompletionUpdate> response =
            chatClient.CompleteChatStreamingAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 100,
                Temperature = 0.7f,
                TopP = 0.95f
            });

        await foreach (StreamingChatCompletionUpdate update in response.ConfigureAwait(false))
        {
            if (update.ContentUpdate.Count > 0)
            {
                yield return new ChatDelta(update.ContentUpdate[0].Text);
            }
        }
    }
}

public record ChatRequest(string Prompt);

public record ChatDelta(string Response);
