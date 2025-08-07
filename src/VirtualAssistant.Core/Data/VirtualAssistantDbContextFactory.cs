// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtualAssistant.Core.Data;

public class VirtualAssistantDbContextFactory : IDesignTimeDbContextFactory<VirtualAssistantDbContext>
{
    public VirtualAssistantDbContext CreateDbContext(string[] args)
    {
        string dbPath = VirtualAssistantDbContext.GetDatabasePath();
        string connectionString = $"Data Source={dbPath}";

        var optionsBuilder = new DbContextOptionsBuilder<VirtualAssistantDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new(optionsBuilder.Options);
    }
}
