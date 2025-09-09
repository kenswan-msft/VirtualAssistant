using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace VirtualAssistant.Web.Client.Chats;

public class ChatService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async IAsyncEnumerable<string> StreamAsync(
        Guid conversationId,
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpRequestMessage =
            new HttpRequestMessage(HttpMethod.Post, $"/api/ai/chat/{conversationId}");

        httpRequestMessage.Content = JsonContent.Create(new ChatRequest(Prompt: message));

        using HttpResponseMessage resp = await httpClient.SendAsync(httpRequestMessage,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        resp.EnsureSuccessStatusCode();

        await foreach (ChatDelta delta in JsonSerializer.DeserializeAsyncEnumerable<ChatDelta>(
                           await resp.Content.ReadAsStreamAsync(cancellationToken),
                           jsonSerializerOptions,
                           cancellationToken))
        {
            if (delta is not null && !string.IsNullOrEmpty(delta.Response))
            {
                yield return delta.Response;
            }
        }
    }

    private record ChatRequest(string Prompt);

    private record ChatDelta(string Response);
}
