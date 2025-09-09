using AutomationIoC.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.CommandLine;

namespace VirtualAssistant.Cli.Chats.Commands;

public class OpenChatCommand : IAutomationCommand
{
    public void Initialize(AutomationCommand command)
    {
        Option<string> modelOption = new("--modelId", "-m")
        {
            Description = "Chat Model (uses Docker Model Runner gpt-oss if not specified).",
            Required = false
        };

        command.Options.Add(modelOption);

        command.SetAction(async (parseResult, context, cancellationToken) =>
        {
            // Service Identifier for the chat completion service
            string? modelId = parseResult.GetValue(modelOption);

            Kernel kernel = context.ServiceProvider.GetRequiredService<Kernel>();

            IChatCompletionService chatCompletionService = kernel.Services
                .GetRequiredKeyedService<IChatCompletionService>(modelId ?? "docker:ai/gpt-oss");

            await ChatAsync(
                kernel,
                chatCompletionService,
                cancellationToken).ConfigureAwait(false);
        });
    }

    private static async Task<int> ChatAsync(
        Kernel kernel,
        IChatCompletionService chatCompletionService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddDeveloperMessage("You are a helpful assistant.");
            chatHistory.AddUserMessage("Hello, who are you?");

            var promptExecutionSettings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.7f,
                    ["max_tokens"] = 4096,
                    ["top_p"] = 0.95f
                }
            };

            IAsyncEnumerable<StreamingChatMessageContent> response =
                chatCompletionService.GetStreamingChatMessageContentsAsync(
                    chatHistory,
                    executionSettings: promptExecutionSettings,
                    kernel: kernel,
                    cancellationToken);

            // TODO: Enable function calling from first chat completion response
            // IAsyncEnumerable<StreamingKernelContent> functionResponse =
            //     kernel.InvokeStreamingAsync(
            //         pluginName: null,
            //         functionName: null,
            //         new(promptExecutionSettings),
            //         cancellationToken);

            await foreach (StreamingChatMessageContent update in response.ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(update.Content))
                {
                    Console.WriteLine(update.Content);
                }
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
            return 1;
        }
        catch (Exception exception)
        {
            Console.WriteLine("An error occurred: " + exception.Message);
            Console.WriteLine("Please try again later.");

            return 1;
        }
    }
}
