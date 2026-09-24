// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;

namespace Vercel.AI.Tests;

internal sealed class ScriptedHandler : HttpMessageHandler
{
    public List<HttpStatusCode> Statuses { get; } = new();

    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        var status = Statuses.Count > 0 ? Statuses[Math.Min(Calls - 1, Statuses.Count - 1)] : HttpStatusCode.OK;
        var json = status == HttpStatusCode.OK ? BodyFor(Uri) : "{\"error\":{\"message\":\"nope\"}}";
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private static string BodyFor(string uri)
    {
        if (uri.Contains("language-model"))
        {
            return "{\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],\"finishReason\":\"stop\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";
        }

        if (uri.Contains("embedding-model"))
        {
            return "{\"embeddings\":[[0.25,0.5]],\"usage\":{\"tokens\":2}}";
        }

        if (uri.Contains("image-model"))
        {
            return "{\"images\":[{\"url\":\"https://example.test/cat.png\"}]}";
        }

        if (uri.Contains("/responses"))
        {
            return "{\"output_text\":\"ok\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"ok\"}]}],\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}";
        }

        if (uri.Contains("/v1/messages"))
        {
            return "{\"id\":\"msg\",\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";
        }

        if (uri.Contains("embedContent"))
        {
            return "{\"embedding\":{\"values\":[0.25,0.5]}}";
        }

        if (uri.Contains("generateContent"))
        {
            return "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"ok\"}]},\"finishReason\":\"STOP\"}]}";
        }

        if (uri.Contains("/converse"))
        {
            return "{\"output\":{\"message\":{\"content\":[{\"text\":\"ok\"}]}},\"stopReason\":\"end_turn\",\"usage\":{\"inputTokens\":1,\"outputTokens\":1,\"totalTokens\":2}}";
        }

        if (uri.EndsWith("/chat", StringComparison.Ordinal) || uri.Contains("/chat?"))
        {
            return "{\"message\":{\"content\":[{\"type\":\"text\",\"text\":\"ok\"}]},\"finish_reason\":\"COMPLETE\"}";
        }

        if (uri.EndsWith("/embed", StringComparison.Ordinal))
        {
            return "{\"embeddings\":{\"float\":[[0.25,0.5]]}}";
        }

        return "{\"text\":\"hello\",\"url\":\"https://example.test/out.bin\",\"id\":\"id_1\",\"status\":\"completed\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2},\"data\":[{\"embedding\":[0.25,0.5]}],\"results\":[{\"index\":0,\"relevance_score\":0.9}]}";
    }
}
