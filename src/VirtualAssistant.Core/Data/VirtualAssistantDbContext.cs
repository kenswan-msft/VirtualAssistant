// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace VirtualAssistant.Core.Data;

public class VirtualAssistantDbContext(DbContextOptions<VirtualAssistantDbContext> options) : DbContext(options)
{
    public DbSet<LocalAiModelEntity> LocalAiModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LocalAiModelEntity>(entity =>
        {
            entity.HasKey(model => model.Id);

            entity.Property(model => model.Name)
                .HasMaxLength(50);

            entity.HasIndex(model => model.Name).IsUnique();

            entity.Property(model => model.Description)
                .HasMaxLength(200);

            entity.Property(model => model.Path)
                .HasMaxLength(300);
        });

    internal static string GetDatabasePath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = Path.Combine(appDataPath, "virtual-assistant-cli");

        // Ensure the directory exists
        Directory.CreateDirectory(appFolder);

        return Path.Combine(appFolder, "virtual-assistant.db");
    }
}
