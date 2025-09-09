using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace VirtualAssistant.Cli.Chats.Services;

public class LocalOpenAIChatAdapter : IChatCompletionService
{
    private readonly ChatClient chatClient;

    public LocalOpenAIChatAdapter()
    {
        var apiKeyCredential = new ApiKeyCredential("not-needed-for-local");

        chatClient = new ChatClient("ai/gpt-oss", apiKeyCredential, new OpenAIClientOptions
        {
            Endpoint = new Uri("http://localhost:12434/engines/v1")
        });
    }

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>
    {
        ["model"] = "local-openai"
    };

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, CancellationToken cancellationToken = new())
    {
        var response = new StringBuilder();

        await foreach (StreamingChatMessageContent? streamingChatMessageContent in
                       GetStreamingChatMessageContentsAsync(
                               chatHistory,
                               executionSettings,
                               kernel,
                               cancellationToken)
                           .ConfigureAwait(false))
        {
            response.Append(streamingChatMessageContent.Content);
        }

        return
        [
            new ChatMessageContent(role: AuthorRole.Assistant, content: response.ToString(), modelId: null,
                innerContent: null, encoding: null, metadata: null)
        ];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("Hello, who are you?")
        };

        AsyncCollectionResult<StreamingChatCompletionUpdate> response =
            chatClient.CompleteChatStreamingAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 100,
                Temperature = 0.7f,
                TopP = 0.95f
            }, cancellationToken);

        await foreach (StreamingChatCompletionUpdate update in response.ConfigureAwait(false))
        {
            if (update.ContentUpdate.Count > 0)
            {
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, update.ContentUpdate[0].Text);
            }
        }
    }
}
