// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

internal sealed class CapturedRequest
{
    public CapturedRequest(Uri? uri, string method, string body, Dictionary<string, string> headers)
    {
        Uri = uri;
        Method = method;
        Body = body;
        Headers = headers;
    }

    public Uri? Uri { get; }

    public string Method { get; }

    public string Body { get; }

    public Dictionary<string, string> Headers { get; }
}

internal sealed class UpstreamCapture : HttpMessageHandler
{
    public List<CapturedRequest> Requests { get; } = new();

    public string ResponseBody { get; set; } = "{\"id\":\"resp-1\",\"model\":\"m\",\"created\":1711363706,\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Hello\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5,\"total_tokens\":15}}";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string MediaType { get; set; } = "application/json";

    public Dictionary<string, string> ResponseHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Queue<string> Bodies { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        Requests.Add(new CapturedRequest(request.RequestUri, request.Method.Method, body, headers));
        var payload = Bodies.Count > 0 ? Bodies.Dequeue() : ResponseBody;
        var message = new HttpResponseMessage(Status)
        {
            Content = new StringContent(payload ?? string.Empty, Encoding.UTF8, MediaType),
        };
        foreach (var pair in ResponseHeaders)
        {
            if (!message.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
            {
                message.Content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        return message;
    }
}

internal static class UpstreamChat
{
    public static OpenAICompatibleLanguageModel Model(UpstreamCapture capture, string name = "openai-compatible", string modelId = "gpt-test", Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new OpenAICompatibleOptions
        {
            ProviderName = name,
            BaseUrl = "https://example.test/v1",
            ApiKey = "secret",
        };
        configure?.Invoke(options);
        return OpenAICompatibleProvider.Create(options, capture).LanguageModel(modelId) as OpenAICompatibleLanguageModel
            ?? throw new InvalidOperationException("Expected a chat model.");
    }

    public static LanguageModelCallOptions Prompt(string text = "Hello")
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(text) },
        };
    }

    public static JsonObject Body(UpstreamCapture capture)
    {
        return JsonNode.Parse(capture.Requests[0].Body)!.AsObject();
    }

    public static async Task<List<LanguageModelStreamPart>> Read(IAsyncEnumerable<LanguageModelStreamPart> stream)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream.ConfigureAwait(false))
        {
            parts.Add(part);
        }

        return parts;
    }

    public static string Sse(params string[] events)
    {
        var builder = new StringBuilder();
        foreach (var item in events)
        {
            builder.Append("data: ");
            builder.Append(item);
            builder.Append("\n\n");
        }

        builder.Append("data: [DONE]\n\n");
        return builder.ToString();
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static Dictionary<string, JsonElement> Bag(string key, string json)
    {
        return new Dictionary<string, JsonElement> { [key] = Json(json) };
    }
}
