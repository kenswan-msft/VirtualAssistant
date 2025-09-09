using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using System.Diagnostics.CodeAnalysis;

namespace VirtualAssistant.Cli.Chats.Services;

public class ChatCompletionServiceFactory : IAIServiceSelector
{
    public bool TrySelectAIService<T>(
        Kernel kernel,
        KernelFunction function,
        KernelArguments arguments,
        [NotNullWhen(true)] out T? service,
        out PromptExecutionSettings? serviceSettings)
        where T : class, IAIService => throw new NotImplementedException();
}
