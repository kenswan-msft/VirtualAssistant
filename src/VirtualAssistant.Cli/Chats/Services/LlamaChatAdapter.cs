using LLama;
using LLama.Common;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text;
using VirtualAssistant.Cli.Configurations.Models;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace VirtualAssistant.Cli.Chats.Services;

public class LlamaChatAdapter(IOptions<LocalAiModelOptions> options) : IChatCompletionService
{
    private readonly InferenceParams inferenceParams = new()
    {
        MaxTokens = 4096,
        AntiPrompts = ["###END###"]
    };

    // private readonly LocalAiModelOptions localAiModelOptions = options.Value;
    private InteractiveExecutor? interactiveExecutor;

    public IReadOnlyDictionary<string, object?> Attributes { get; } =
        new Dictionary<string, object?> { ["serviceId"] = "llama" };

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
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
            new(
                role: AuthorRole.Assistant, content: response.ToString(), modelId: null,
                innerContent: null, encoding: null, metadata: null)
        ];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string modelPath = "";
        // var modelParameters = new ModelParams(localAiModelOptions.ModelPath)
        Console.WriteLine("Starting to load model from " + options.Value.ContextSize);

        var modelParameters = new ModelParams(modelPath)
        {
            ContextSize = 200,
            // ContextSize = (uint)localAiModelOptions.ContextSize,
            GpuLayerCount = 4
            // GpuLayerCount = localAiModelOptions.GpuLayerCount
        };

        using LLamaWeights llamaWeights =
            await LLamaWeights.LoadFromFileAsync(modelParameters, cancellationToken)
                .ConfigureAwait(false);

        using LLamaContext lLamaContext = llamaWeights.CreateContext(modelParameters);
        interactiveExecutor = new(lLamaContext);
        LLama.Common.ChatHistory llamaChatHistory = ConvertToLlamaChatHistory(chatHistory);
        int lastMessageIndex = llamaChatHistory.Messages.Count - 1;
        LLama.Common.ChatHistory.Message userMessage = llamaChatHistory.Messages[lastMessageIndex];
        llamaChatHistory.Messages.RemoveAt(lastMessageIndex);

        var session = new ChatSession(interactiveExecutor, llamaChatHistory);

        if (string.IsNullOrWhiteSpace(userMessage.Content))
        {
            yield break;
        }

        await foreach (string? text in session.ChatAsync(
                           userMessage,
                           inferenceParams,
                           cancellationToken).ConfigureAwait(false))
        {
            yield return new(AuthorRole.Assistant, text);
        }
    }

    private static LLama.Common.ChatHistory ConvertToLlamaChatHistory(ChatHistory chatHistory)
    {
        var llamaHistory = new LLama.Common.ChatHistory();

        foreach (ChatMessageContent? message in chatHistory)
        {
            LLama.Common.AuthorRole role = message.Role.Label switch
            {
                "user" => LLama.Common.AuthorRole.User,
                "assistant" => LLama.Common.AuthorRole.Assistant,
                "system" => LLama.Common.AuthorRole.System,
                _ => LLama.Common.AuthorRole.User
            };

            llamaHistory.AddMessage(role, message.Content ?? string.Empty);
        }

        return llamaHistory;
    }
}
