// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using AutomationIoC.CommandLine;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using VirtualAssistant.Core.Configuration;

namespace VirtualAssistant.Cli.Commands;

public class OpenChatCommand : IAutomationCommand
{
    public void Initialize(AutomationCommand command) =>
        command.SetAction(async (_, context, cancellationToken) =>
        {
            LocalAiModelOptions localAiModelOptions =
                context.ServiceProvider.GetRequiredService<IOptions<LocalAiModelOptions>>().Value;

            var modelParameters = new ModelParams(localAiModelOptions.Model.Path)
            {
                ContextSize = (uint)localAiModelOptions.ContextSize,
                GpuLayerCount = localAiModelOptions.GpuLayerCount
            };

            using LLamaWeights llamaWeights =
                await LLamaWeights.LoadFromFileAsync(modelParameters, cancellationToken)
                    .ConfigureAwait(false);

            using LLamaContext lLamaContext = llamaWeights.CreateContext(modelParameters);
            var interactiveExecutor = new InteractiveExecutor(lLamaContext);

            await ChatAsync(interactiveExecutor, cancellationToken).ConfigureAwait(false);
        });

    private static async Task ChatAsync(
        InteractiveExecutor interactiveExecutor,
        CancellationToken cancellationToken)
    {
        try
        {
            string[] userCancelInput = ["exit", "quit", "cancel", "stop", "end", "close"];
            string[] trimWords = ["Assistant:", "User:", "System:", "<|assistant|>", "<|user|>", "<|system|>"];
            const string assistantIntroduction = "What can I help you with today?";

            var chatHistory = new ChatHistory();

            chatHistory.AddMessage(AuthorRole.System, "You are a helpful assistant.");

            chatHistory.AddMessage(AuthorRole.Assistant, assistantIntroduction);

            ChatSession session = new(interactiveExecutor, chatHistory);

            var inferenceParams = new InferenceParams
            {
                // No more than 256 tokens should appear in answer.
                // Remove it if anti-prompt is enough for control.
                MaxTokens = 4096,

                // Stop generation once anti-prompts appear.
                AntiPrompts = ["User:", "System:"],
                SamplingPipeline = new DefaultSamplingPipeline()
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The chat session has started.\n");
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"{assistantIntroduction} (type 'exit' to quit).\n");
            Console.ForegroundColor = ConsoleColor.Green;
            string userPrompt = null;

            do
            {
                while (string.IsNullOrWhiteSpace(userPrompt) && !cancellationToken.IsCancellationRequested)
                {
                    userPrompt = Console.ReadLine();
                }

                if (cancellationToken.IsCancellationRequested ||
                    userCancelInput.Any(cancelInput =>
                        userPrompt?.Equals(cancelInput, StringComparison.OrdinalIgnoreCase) == true))
                {
                    break;
                }

                // Generate the response streamingly.
                var assistantResponseBuilder = new StringBuilder();
                Console.ForegroundColor = ConsoleColor.Cyan;

                await foreach (string text in session.ChatAsync(
                                   new ChatHistory.Message(AuthorRole.User, userPrompt),
                                   inferenceParams, cancellationToken).ConfigureAwait(true))
                {
                    if (!trimWords.Contains(text.Trim(), StringComparer.OrdinalIgnoreCase))
                    {
                        Console.Write(text);
                    }

                    assistantResponseBuilder.Append(text);
                }

                // Add question/response to chat context
                chatHistory.AddMessage(AuthorRole.User, userPrompt);
                chatHistory.AddMessage(AuthorRole.Assistant, assistantResponseBuilder.ToString());

                Console.ForegroundColor = ConsoleColor.Green;
                userPrompt = null;
                Console.WriteLine();
            } while (!cancellationToken.IsCancellationRequested);

            Console.WriteLine("Goodbye!");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
        }
        catch (Exception exception)
        {
            Console.WriteLine("An error occurred: " + exception.Message);
            Console.WriteLine("Please try again later.");
            throw;
        }
    }
}
