using VirtualAssistant.Cli.Configurations.Models;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace VirtualAssistant.Cli.Chats.Services;

public class LlamaChatAdapter(IOptions<LocalAiModelOptions> options) : IChatCompletionService
{
    private readonly InferenceParams inferenceParams = new()
    {
        MaxTokens = 4096,
        AntiPrompts = ["User:", "System:"],
        SamplingPipeline = new DefaultSamplingPipeline()
    };

    private readonly LocalAiModelOptions localAiModelOptions = options.Value;
    private InteractiveExecutor? interactiveExecutor;

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>
    {
        ["model"] = "local-llamasharp"
    };

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Use executionSettings to configure model
        await InitializeInteractiveExecutorAsync(cancellationToken).ConfigureAwait(false);

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
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await InitializeInteractiveExecutorAsync(cancellationToken).ConfigureAwait(false);

        LLama.Common.ChatHistory llamaChatHistory = ConvertToLlamaChatHistory(chatHistory);
        var session = new ChatSession(interactiveExecutor, llamaChatHistory);

        ChatMessageContent lastMessage = chatHistory.LastOrDefault();
        if (lastMessage == null)
        {
            yield break;
        }

        await foreach (string? text in session.ChatAsync(
                           new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User,
                               lastMessage.Content ?? string.Empty),
                           inferenceParams,
                           cancellationToken).ConfigureAwait(false))
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, text);
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

    private async Task InitializeInteractiveExecutorAsync(CancellationToken cancellationToken = default)
    {
        if (interactiveExecutor is not null)
        {
            return;
        }

        var modelParameters = new ModelParams(localAiModelOptions.ModelPath)
        {
            ContextSize = (uint)localAiModelOptions.ContextSize,
            GpuLayerCount = localAiModelOptions.GpuLayerCount
        };

        using LLamaWeights llamaWeights =
            await LLamaWeights.LoadFromFileAsync(modelParameters, cancellationToken)
                .ConfigureAwait(false);

        using LLamaContext lLamaContext = llamaWeights.CreateContext(modelParameters);
        interactiveExecutor = new InteractiveExecutor(lLamaContext);
    }
}
