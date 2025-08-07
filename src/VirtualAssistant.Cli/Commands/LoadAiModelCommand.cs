// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using AutomationIoC.CommandLine;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json;
using VirtualAssistant.Core.Data;

namespace VirtualAssistant.Cli.Commands;

public class LoadAiModelCommand : IAutomationCommand
{
    public void Initialize(AutomationCommand command)
    {
        Option<string> nameOption = new("--name", "-n")
        {
            Description = "Name of model",
            Required = true
        };

        Option<string> descriptionOption = new("--description", "-d")
        {
            Description = "Description of model",
            Required = false
        };

        Option<string> pathOption = new("--path", "-p")
        {
            Description = "File path to model",
            Required = true
        };

        command.Options.Add(nameOption);
        command.Options.Add(descriptionOption);
        command.Options.Add(pathOption);

        command.SetAction(async (parseResult, context, cancellationToken) =>
        {
            string name = parseResult.GetValue(nameOption);
            string description = parseResult.GetValue(descriptionOption);
            string path = parseResult.GetValue(pathOption);

            using IServiceScope serviceScope = context.ServiceProvider.CreateScope();

            VirtualAssistantDbContext virtualAssistantDbContext =
                serviceScope.ServiceProvider.GetRequiredService<VirtualAssistantDbContext>();

            JsonSerializerOptions jsonSerializerOptions =
                serviceScope.ServiceProvider.GetKeyedService<JsonSerializerOptions>(JsonSerializerOptionKeys.Console);

            EntityEntry<LocalAiModelEntity>? addedModel =
                await virtualAssistantDbContext.LocalAiModels.AddAsync(
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Path = path,
                        IsActive = true
                    }, cancellationToken).ConfigureAwait(false);

            await virtualAssistantDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        });
    }
}
