using AutomationIoC.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;
using VirtualAssistant.Cli.Chats.Commands;
using VirtualAssistant.Cli.Chats.Services;
using VirtualAssistant.Cli.Configurations.Models;

internal class Program
{
    public static async Task Main(string[] args)
    {
        IAutomationConsoleBuilder builder =
            AutomationConsole.CreateDefaultBuilder<OpenChatCommand>("Virtual Assistant CLI", args);

        builder.Configure((hostBuilderContext, configurationBuilder) => { });

        builder.ConfigureServices((hostBuilderContext, serviceCollection) =>
        {
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

            kernelBuilder.Services.AddOptions<LocalAiModelOptions>()
                .BindConfiguration(nameof(LocalAiModelOptions));

            kernelBuilder.AddOpenAIChatCompletion(
                modelId: "ai/gpt-oss",
                endpoint: new("http://localhost:12434/engines/v1"),
                apiKey: "not-neded-for-local",
                orgId: null,
                serviceId: "docker",
                httpClient: null);

            // TODO: See if above AddOpenAIChatCompletion works for local dock model runner
            // If not, use the LocalOpenAIChatAdapter implementation below
            // kernelBuilder.Services.AddKeyedScoped<LocalOpenAIChatAdapter>("docker");

            kernelBuilder.Services.AddKeyedScoped<LlamaChatAdapter>("llama");

            serviceCollection.AddScoped(_ => kernelBuilder.Build());

            serviceCollection
                .AddKeyedSingleton(
                    JsonSerializerConfigurations.Console,
                    (serviceProvider, key) =>
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        })
                .AddKeyedSingleton(
                    JsonSerializerConfigurations.Web,
                    (serviceProvider, key) =>
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));
        });

        IAutomationConsole application = builder.Build();

        await application.RunAsync().ConfigureAwait(false);
    }
}

internal enum JsonSerializerConfigurations
{
    Console,
    Web
}
