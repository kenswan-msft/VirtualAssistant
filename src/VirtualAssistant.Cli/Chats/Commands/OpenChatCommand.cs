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
            Description = "Determines chat model to use.",
            Required = false
        };

        Option<string> serviceOption = new("--serviceId", "-s")
        {
            Description = "Determines chat service to use. (.e.g. docker, llama)",
            Required = false
        };

        command.Options.Add(modelOption);
        command.Options.Add(serviceOption);

        command.SetAction(async (parseResult, context, cancellationToken) =>
        {
            string? modelId = parseResult.GetValue(modelOption);
            string? serviceId = parseResult.GetValue(serviceOption);

            Kernel kernel = context.ServiceProvider.GetRequiredService<Kernel>();

            IChatCompletionService chatCompletionService = kernel.Services
                .GetRequiredKeyedService<IChatCompletionService>(serviceId ?? "llama");

            var promptExecutionSettings = new PromptExecutionSettings
            {
                ModelId = modelId ?? "ai/gpt-oss",
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.7f,
                    ["max_tokens"] = 4096,
                    ["top_p"] = 0.95f
                }
            };

            await ChatAsync(
                kernel,
                promptExecutionSettings,
                chatCompletionService,
                cancellationToken).ConfigureAwait(false);
        });
    }

    private static async Task<int> ChatAsync(
        Kernel kernel,
        PromptExecutionSettings? promptExecutionSettings,
        IChatCompletionService chatCompletionService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatHistory = new ChatHistory();
            const string endOfMessageIndicator = "###END###";

            chatHistory.AddSystemMessage(
                "You are a helpful assistant. When you are finished with your message" +
                $" please respond with '{endOfMessageIndicator}' on a new line by itself.");

            Console.WriteLine("Welcome to the Virtual Assistant CLI!");
            Console.WriteLine("Type your message and press Enter to chat. Type 'exit' to quit.");

            string userMessage = Console.ReadLine();
            chatHistory.AddUserMessage(userMessage);

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
                    Console.Write(update.Content);
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
