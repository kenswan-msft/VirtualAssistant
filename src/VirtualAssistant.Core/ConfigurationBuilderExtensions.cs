// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VirtualAssistant.Core.Configuration;
using VirtualAssistant.Core.Data;

namespace VirtualAssistant.Core;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddVirtualAssistantConfiguration(this IConfigurationBuilder builder)
    {
        string connectionString = $"Data Source={VirtualAssistantDbContext.GetDatabasePath()}";

        DbContextOptions<VirtualAssistantDbContext> options =
            new DbContextOptionsBuilder<VirtualAssistantDbContext>()
                .UseSqlite(connectionString)
                .Options;

        builder.Add(new EntityFrameworkConfigurationSource(options));

        return builder;
    }
}
