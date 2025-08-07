// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VirtualAssistant.Core.Data;

namespace VirtualAssistant.Core.Configuration;

public class EntityFrameworkConfigurationProvider(
    DbContextOptions<VirtualAssistantDbContext> options) : ConfigurationProvider
{
    public override void Load()
    {
        using var context = new VirtualAssistantDbContext(options);
        context.Database.Migrate();

        // Load active environment and application ID
        LocalAiModelEntity? activeAiModel =
            context.LocalAiModels
                .FirstOrDefault(model => model.IsActive);

        if (activeAiModel is null || activeAiModel.Id == Guid.Empty)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No active local AI model found in the configuration.");
            Console.WriteLine("Please run 'load' command to set up the active AI model.");
            Console.WriteLine("If this is currently the init command, please ignore and continue setup.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(
                $"Running active model: {activeAiModel.Name} - (Path: {activeAiModel.Path})");

            Console.ResetColor();

            Data = new Dictionary<string, string>
            {
                { "LocalAiModelOptions:Model:Id", activeAiModel.Id.ToString() },
                { "LocalAiModelOptions:Model:Name", activeAiModel.Name },
                { "LocalAiModelOptions:Model:Description", activeAiModel.Name },
                { "LocalAiModelOptions:Model:Path", activeAiModel.Path }
            };
        }
    }
}
