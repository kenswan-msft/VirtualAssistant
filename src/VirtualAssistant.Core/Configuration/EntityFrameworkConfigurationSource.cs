// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VirtualAssistant.Core.Data;

namespace VirtualAssistant.Core.Configuration;

public class EntityFrameworkConfigurationSource(
    DbContextOptions<VirtualAssistantDbContext> options) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EntityFrameworkConfigurationProvider(options);
}
