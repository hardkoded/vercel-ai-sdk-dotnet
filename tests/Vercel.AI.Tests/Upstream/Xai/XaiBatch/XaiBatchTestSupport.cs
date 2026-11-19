// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using XaiProviderType = Vercel.AI.Xai.XaiProvider;

namespace Vercel.AI.Tests.Upstream.Xai.XaiBatch;

/// <summary>One request the test server saw.</summary>
internal sealed class XaiBatchCall
{
    public required HttpMethod Method { get; init; }

    public required string Url { get; init; }

    public required Dictionary<string, string> Headers { get; init; }

    public required byte[] Body { get; init; }

    public string? ContentType { get; init; }

    public bool TokenCanBeCancelled { get; init; }

    public JsonNode? Json => JsonNode.Parse(Encoding.UTF8.GetString(Body));

    /// <summary>The parts of a multipart body, in the order they were sent.</summary>
    public List<MultipartField> Multipart()
    {
        var boundary = ContentType!.Split("boundary=")[1].Trim('"', ';', ' ');
        var text = Encoding.UTF8.GetString(Body);
        var fields = new List<MultipartField>();
        foreach (var part in text.Split("--" + boundary))
        {
            var separator = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (separator < 0)
            {
                continue;
            }

            var head = part[..separator];
            var value = part[(separator + 4)..];
            if (value.EndsWith("\r\n", StringComparison.Ordinal))
            {
                value = value[..^2];
            }

            string? Between(string key)
            {
                var start = head.IndexOf(key + "=\"", StringComparison.Ordinal);
                if (start < 0)
                {
                    return null;
                }

                start += key.Length + 2;
                return head[start..head.IndexOf('"', start)];
            }

            var type = head.Split("\r\n")
                .Select(line => line.Split(": ", 2))
                .Where(pair => pair[0].Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                .Select(pair => pair[1])
                .FirstOrDefault();
            fields.Add(new MultipartField(Between("name")!, Between("filename"), type, value));
        }

        return fields;
    }
}

internal sealed record MultipartField(
    string Name,
    string? FileName,
    string? ContentType,
    string Value
);

/// <summary>Answers each URL with a registered body and records every call, like upstream's <c>createTestServer</c>.</summary>
internal sealed class XaiBatchServer : HttpMessageHandler
{
    private readonly Dictionary<string, Func<int, string>> _responses = new();
    private readonly Dictionary<string, int> _counts = new();

    public List<XaiBatchCall> Calls { get; } = new();

    public XaiBatchServer Respond(string url, object body)
    {
        var text = body is string raw ? raw : JsonSerializer.Serialize(body);
        _responses[url] = _ => text;
        return this;
    }

    public XaiBatchServer Respond(string url, Func<int, string> body)
    {
        _responses[url] = body;
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var url = request.RequestUri!.AbsoluteUri;
        var headers = request.Headers.ToDictionary(
            pair => pair.Key.ToLowerInvariant(),
            pair => string.Join(",", pair.Value)
        );
        Calls.Add(
            new XaiBatchCall
            {
                Method = request.Method,
                Url = url,
                Headers = headers,
                Body =
                    request.Content == null
                        ? Array.Empty<byte>()
                        : await request.Content.ReadAsByteArrayAsync(CancellationToken.None),
                ContentType = request.Content?.Headers.ContentType?.ToString(),
                TokenCanBeCancelled = cancellationToken.CanBeCanceled,
            }
        );
        var key = url.Split('?')[0];
        _counts[key] = _counts.GetValueOrDefault(key) + 1;
        if (!_responses.TryGetValue(key, out var body) && !_responses.TryGetValue(url, out body))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"error\":\"no route for " + url + "\"}"),
            };
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body(_counts[key]), Encoding.UTF8, "application/json"),
        };
    }
}

internal static class XaiBatchTestSupport
{
    internal const string Files = "https://api.x.ai/v1/files";
    internal const string Batches = "https://api.x.ai/v1/batches";
    internal const string BatchUrl = "https://api.x.ai/v1/batches/batch_123";
    internal const string Cancel = "https://api.x.ai/v1/batches/batch_123:cancel";
    internal const string Results = "https://api.x.ai/v1/batches/batch_123/results";
    internal const string ResultsPage1 = "https://api.x.ai/v1/batches/batch_123/results?limit=1000";
    internal const string ResultsPage2 =
        "https://api.x.ai/v1/batches/batch_123/results?limit=1000&pagination_token=next%2Fpage";

    internal static IBatchApi CreateBatch(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return XaiProviderType.Create(options, handler).ExperimentalBatch()!;
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static JsonElement Json(JsonNode node) => Json(node.ToJsonString());

    /// <summary>Port of the test file's <c>request</c> helper.</summary>
    internal static NormalizedBatchRequest Request(
        string id,
        string prompt,
        string modelId = "grok-4.3",
        int? topK = null
    )
    {
        var options = new JsonObject
        {
            ["prompt"] = new JsonArray(
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = new JsonArray(
                        new JsonObject { ["type"] = "text", ["text"] = prompt }
                    ),
                }
            ),
        };
        if (topK != null)
        {
            options["topK"] = topK;
        }

        return new NormalizedBatchRequest(id, "text", modelId, Json(options));
    }

    internal static BatchOperationCall Call(
        string? batchId = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default,
        int? limit = null,
        string? cursor = null,
        string? webhookUrl = null,
        JsonElement? providerOptions = null,
        IReadOnlyList<NormalizedBatchRequest>? requests = null
    ) =>
        new(
            batchId,
            providerOptions,
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            limit,
            cursor,
            webhookUrl,
            requests
        );

    /// <summary>Port of the test file's <c>batchResponse</c> helper.</summary>
    internal static JsonObject BatchResponse(
        string batchId = "batch_123",
        JsonObject? state = null,
        string? cancelTime = null,
        string? cancelByXaiMessage = null
    ) =>
        new()
        {
            ["batch_id"] = batchId,
            ["name"] = "ai-sdk-text-batch",
            ["create_time"] = "2026-08-25T12:00:00Z",
            ["expire_time"] = "2099-08-26T12:00:00Z",
            ["cancel_time"] = cancelTime,
            ["cancel_by_xai_message"] = cancelByXaiMessage,
            ["state"] = state
                ?? new JsonObject
                {
                    ["num_requests"] = 2,
                    ["num_pending"] = 0,
                    ["num_success"] = 2,
                    ["num_error"] = 0,
                    ["num_cancelled"] = 0,
                },
        };

    internal static JsonObject State(
        int requests,
        int pending,
        int success,
        int error,
        int cancelled
    ) =>
        new()
        {
            ["num_requests"] = requests,
            ["num_pending"] = pending,
            ["num_success"] = success,
            ["num_error"] = error,
            ["num_cancelled"] = cancelled,
        };

    /// <summary>Port of the test file's <c>chatResultBody</c> helper.</summary>
    internal static JsonObject ChatResultBody(string text) =>
        JsonNode
            .Parse(
                new JsonObject
                {
                    ["id"] = "response_123",
                    ["object"] = "chat.completion",
                    ["created"] = 1_700_000_000,
                    ["model"] = "grok-4.3",
                    ["choices"] = new JsonArray(
                        new JsonObject
                        {
                            ["index"] = 0,
                            ["message"] = new JsonObject
                            {
                                ["role"] = "assistant",
                                ["content"] = text,
                                ["reasoning_content"] = "Reasoning",
                                ["tool_calls"] = null,
                            },
                            ["finish_reason"] = "stop",
                        }
                    ),
                    ["usage"] = new JsonObject
                    {
                        ["prompt_tokens"] = 10,
                        ["completion_tokens"] = 3,
                        ["total_tokens"] = 14,
                        ["prompt_tokens_details"] = new JsonObject { ["cached_tokens"] = 2 },
                        ["completion_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 1 },
                        ["cost_in_usd_ticks"] = 123,
                    },
                    ["citations"] = new JsonArray("https://example.com/source"),
                    ["service_tier"] = "default",
                }.ToJsonString()
            )!
            .AsObject();

    /// <summary>Port of the test file's <c>successfulResult</c> helper.</summary>
    internal static JsonObject SuccessfulResult(string id, string text) =>
        new()
        {
            ["batch_request_id"] = id,
            ["batch_result"] = new JsonObject
            {
                ["response"] = new JsonObject { ["chat_get_completion"] = ChatResultBody(text) },
                ["error"] = new JsonObject { ["code"] = 0, ["message"] = "" },
            },
        };

    internal static async Task<List<BatchItem>> ReadAsync(IAsyncEnumerable<BatchItem> stream)
    {
        var items = new List<BatchItem>();
        await foreach (var item in stream)
        {
            items.Add(item);
        }

        return items;
    }
}
