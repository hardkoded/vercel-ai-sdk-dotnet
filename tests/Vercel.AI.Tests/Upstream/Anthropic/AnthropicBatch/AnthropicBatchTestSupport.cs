// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch;

internal static class AnthropicBatchTestSupport
{
    internal const string BatchesUrl = "https://api.anthropic.com/v1/messages/batches";
    internal const string ResultsUrl =
        "https://api.anthropic.com/v1/messages/batches/msgbatch_123/results";

    internal static IBatchApi CreateBatch(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? headers = null,
        Func<string>? generateId = null
    )
    {
        var options = new AnthropicOptions { ApiKey = "test-api-key", GenerateId = generateId };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return AnthropicProvider.Create(options, handler).ExperimentalBatch()!;
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static JsonArray Messages(string prompt) =>
        new(
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = new JsonArray(
                    new JsonObject { ["type"] = "text", ["text"] = prompt }
                ),
            }
        );

    internal static NormalizedBatchRequest TextRequest(
        string id,
        string prompt,
        JsonObject? extra = null,
        string modelId = "claude-3-haiku-20240307"
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
        foreach (var pair in extra ?? new JsonObject())
        {
            options[pair.Key] = pair.Value?.DeepClone();
        }

        return new NormalizedBatchRequest(id, "text", modelId, Json(options.ToJsonString()));
    }

    internal static BatchOperationCall StartCall(NormalizedBatchRequest request) =>
        StartCall([request]);

    internal static BatchOperationCall StartCall(
        IReadOnlyList<NormalizedBatchRequest> requests,
        JsonObject? providerOptions = null,
        IReadOnlyDictionary<string, string>? headers = null,
        string? webhookUrl = null
    ) =>
        new(
            null,
            providerOptions == null ? null : Json(providerOptions.ToJsonString()),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            webhookUrl: webhookUrl,
            requests: requests
        );

    internal static BatchOperationCall OperationCall(
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    ) => new("msgbatch_123", null, headers ?? new Dictionary<string, string>(), cancellationToken);

    internal static JsonObject Counts(
        int processing,
        int succeeded,
        int errored,
        int canceled,
        int expired
    ) =>
        new()
        {
            ["processing"] = processing,
            ["succeeded"] = succeeded,
            ["errored"] = errored,
            ["canceled"] = canceled,
            ["expired"] = expired,
        };

    internal static JsonObject BatchResponse(params (string Key, object? Value)[] overrides)
    {
        var batch = new JsonObject
        {
            ["id"] = "msgbatch_123",
            ["type"] = "message_batch",
            ["processing_status"] = "ended",
            ["request_counts"] = Counts(0, 1, 0, 0, 0),
            ["created_at"] = "2024-09-24T18:37:24.100Z",
            ["expires_at"] = "2024-09-25T18:37:24.100Z",
            ["archived_at"] = null,
            ["cancel_initiated_at"] = null,
            ["ended_at"] = "2024-09-24T18:38:24.100Z",
            ["results_url"] = ResultsUrl,
        };
        foreach (var (key, value) in overrides)
        {
            batch[key] = value is JsonNode node ? node : JsonValue.Create(value as string);
        }

        return batch;
    }

    internal static JsonObject MessageResultBody(string text) =>
        new()
        {
            ["id"] = "msg_123",
            ["type"] = "message",
            ["role"] = "assistant",
            ["model"] = "claude-3-haiku-20240307",
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text }),
            ["stop_reason"] = "end_turn",
            ["stop_sequence"] = null,
            ["usage"] = new JsonObject
            {
                ["input_tokens"] = 10,
                ["output_tokens"] = 3,
                ["cache_creation_input_tokens"] = 1,
                ["cache_read_input_tokens"] = 2,
            },
        };

    /// <summary>The message body with <paramref name="content"/> and any other fields replaced.</summary>
    internal static JsonObject MessageWith(
        string text,
        JsonArray content,
        params (string Key, JsonNode? Value)[] overrides
    )
    {
        var message = MessageResultBody(text);
        message["content"] = content;
        foreach (var (key, value) in overrides)
        {
            message[key] = value;
        }

        return message;
    }

    internal static string SucceededLine(string customId, JsonObject message) =>
        new JsonObject
        {
            ["custom_id"] = customId,
            ["result"] = new JsonObject { ["type"] = "succeeded", ["message"] = message },
        }.ToJsonString();

    internal static async Task<List<BatchItem>> GetResultsAsync(
        RecordingHandler handler,
        params string[] lines
    )
    {
        handler.Respond(BatchResponse()).RespondChunks(ResultsUrl, lines);
        var stream = await CreateBatch(handler)
            .DoGetResultsAsync(OperationCall(), CancellationToken.None);
        return await stream.ToListAsync().AsTask();
    }

    internal static BatchTextGeneration Succeeded(BatchItem item)
    {
        Assert.Equal("succeeded", item.Status);
        return item.Text!;
    }

    internal static void AssertJson(string expected, JsonElement? actual)
    {
        Assert.True(actual.HasValue, "Expected JSON, got none.");
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(expected),
                JsonNode.Parse(actual!.Value.GetRawText())
            ),
            "Expected " + expected + " but was " + actual!.Value.GetRawText()
        );
    }

    /// <summary>The item is invalid, as <c>invalidAnthropicBatchResult</c> reports it.</summary>
    internal static void AssertInvalid(BatchItem item, string id)
    {
        Assert.Equal(id, item.Id);
        Assert.Equal("failed", item.Status);
        Assert.Equal("Anthropic returned an invalid Message batch result.", item.ErrorMessage);
        Assert.Equal("invalid_response", item.ErrorCode);
    }
}

internal sealed record RecordedRequest(
    string Method,
    string Url,
    string Body,
    Dictionary<string, string> Headers,
    bool TokenCancelledByCaller,
    bool TokenCanBeCanceled
);

internal sealed class RecordingHandler : HttpMessageHandler
{
    private string _response = "{}";
    private readonly Dictionary<string, string> _routes = new();
    private readonly Dictionary<string, string[]> _chunks = new();

    public List<RecordedRequest> Requests { get; } = [];

    public Action? OnSend { get; set; }

    public RecordingHandler Respond(JsonNode response)
    {
        _response = response.ToJsonString();
        return this;
    }

    public RecordingHandler Respond(string url, string jsonLines)
    {
        _routes[url] = jsonLines;
        return this;
    }

    /// <summary>Serves <paramref name="chunks"/> from <paramref name="url"/>, one network read per chunk.</summary>
    public RecordingHandler RespondChunks(string url, params string[] chunks)
    {
        _chunks[url] = chunks;
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var body =
            request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = request.Headers.ToDictionary(
            header => header.Key,
            header => string.Join(",", header.Value),
            StringComparer.OrdinalIgnoreCase
        );
        OnSend?.Invoke();
        Requests.Add(
            new RecordedRequest(
                request.Method.Method,
                request.RequestUri!.AbsoluteUri,
                body,
                headers,
                cancellationToken.IsCancellationRequested,
                cancellationToken.CanBeCanceled
            )
        );
        if (_chunks.TryGetValue(request.RequestUri.AbsoluteUri, out var chunks))
        {
            var content = new StreamContent(new ChunkedStream(chunks));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/jsonl"
            );
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                _routes.TryGetValue(request.RequestUri.AbsoluteUri, out var routed)
                    ? routed
                    : _response,
                Encoding.UTF8,
                "application/json"
            ),
        };
    }
}

internal sealed class CountingHandler(Action onCall) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        onCall();
        return Task.FromResult(
            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
        );
    }
}

/// <summary>A read-only stream whose every read returns at most one chunk, so a line can span reads.</summary>
internal sealed class ChunkedStream(string[] chunks) : Stream
{
    private int _index;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_index >= chunks.Length)
        {
            return 0;
        }

        var bytes = Encoding.UTF8.GetBytes(chunks[_index++]);
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        return bytes.Length;
    }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
}
