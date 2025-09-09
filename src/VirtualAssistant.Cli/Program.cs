using AutomationIoC.CommandLine;
using VirtualAssistant.Cli.Chats.Commands;
using VirtualAssistant.Cli.Chats.Services;
using VirtualAssistant.Cli.Configurations.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;

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

            kernelBuilder.Services.AddScoped<LlamaChatAdapter>();
            kernelBuilder.Services.AddScoped<LocalOpenAIChatAdapter>();
            kernelBuilder.Services.AddSingleton<IAIServiceSelector, ChatCompletionServiceFactory>();

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
