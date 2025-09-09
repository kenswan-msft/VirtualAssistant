namespace VirtualAssistant.Cli.Configurations.Models;

public class LocalAiModelOptions
{
    public int ContextSize { get; set; } = 4096;

    public int GpuLayerCount { get; set; } = 4;

    public string ModelPath { get; set; }
}
