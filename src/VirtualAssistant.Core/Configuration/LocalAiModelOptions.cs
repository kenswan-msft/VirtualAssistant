// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

namespace VirtualAssistant.Core.Configuration;

public class LocalAiModelOptions
{
    public int ContextSize { get; set; } = 4096;

    public int GpuLayerCount { get; set; } = 4;

    public LocalAiModel Model { get; set; }
}
