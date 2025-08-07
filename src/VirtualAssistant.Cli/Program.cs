// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using AutomationIoC.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using VirtualAssistant.Cli.Commands;
using VirtualAssistant.Core;

namespace VirtualAssistant.Cli;

internal class Program
{
    public static async Task Main(string[] args)
    {
        IAutomationConsoleBuilder builder =
            AutomationConsole.CreateDefaultBuilder<OpenChatCommand>("Virtual Assistant CLI", args);

        builder.AddCommand<LoadAiModelCommand>("model", "load");

        builder.Configure((hostBuilderContext, configurationBuilder) =>
        {
            configurationBuilder.AddVirtualAssistantConfiguration();
        });

        builder.ConfigureServices((hostBuilderContext, serviceCollection) =>
        {
            serviceCollection.AddVirtualAssistantData();

            serviceCollection
                .AddKeyedSingleton(
                    JsonSerializerOptionKeys.Console,
                    (serviceProvider, key) =>
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        })
                .AddKeyedSingleton(
                    JsonSerializerOptionKeys.Web,
                    (serviceProvider, key) =>
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));
        });

        IAutomationConsole application = builder.Build();

        await application.RunAsync().ConfigureAwait(false);
    }
}

internal enum JsonSerializerOptionKeys
{
    Console,
    Web
}
