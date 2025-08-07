// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualAssistant.Core.Configuration;
using VirtualAssistant.Core.Data;

namespace VirtualAssistant.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVirtualAssistantData(
        this IServiceCollection services)
    {
        services.AddDbContext<VirtualAssistantDbContext>(optionsBuilder =>
        {
            string connectionString = $"Data Source={VirtualAssistantDbContext.GetDatabasePath()}";

            optionsBuilder.UseSqlite(connectionString);
        });

        services.AddOptions<LocalAiModelOptions>()
            .BindConfiguration(nameof(LocalAiModelOptions));

        return services;
    }
}
