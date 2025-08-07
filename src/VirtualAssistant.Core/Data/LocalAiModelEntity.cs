// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

namespace VirtualAssistant.Core.Data;

public class LocalAiModelEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Path { get; set; }

    public bool IsActive { get; set; }
}
