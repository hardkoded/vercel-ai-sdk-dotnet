// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIBatch;

internal static class OpenAIBatchTestSupport
{
    public const string BatchUrl = "https://api.openai.com/v1/batches/batch_123";
    public const string OutputUrl = "https://api.openai.com/v1/files/file-output/content";
    public const string ErrorsUrl = "https://api.openai.com/v1/files/file-errors/content";
    public const string FilesUrl = "https://api.openai.com/v1/files";
    public const string BatchesUrl = "https://api.openai.com/v1/batches";
    public const string CancelUrl = "https://api.openai.com/v1/batches/batch_123/cancel";

    public static string BatchResponse(string? outputFileId = null, string? errorFileId = null)
    {
        return BatchObject(outputFileId: outputFileId, errorFileId: errorFileId).ToJsonString();
    }

    public static JsonObject BatchObject(string id = "batch_123", string status = "completed", string? outputFileId = null, string? errorFileId = null, JsonObject? requestCounts = null, JsonObject? errors = null)
    {
        return new JsonObject
        {
            ["id"] = id,
            ["status"] = status,
            ["output_file_id"] = outputFileId,
            ["error_file_id"] = errorFileId,
            ["created_at"] = 1_700_000_000,
            ["expires_at"] = 1_700_086_400,
            ["request_counts"] = requestCounts ?? Counts(2, 2, 0),
            ["errors"] = errors,
        };
    }

    public static JsonObject Counts(int total, int completed, int failed)
    {
        return new JsonObject { ["total"] = total, ["completed"] = completed, ["failed"] = failed };
    }

    public static string UploadResponse()
    {
        return "{\"id\":\"file-input\",\"object\":\"file\",\"filename\":\"batch.jsonl\",\"purpose\":\"batch\",\"expires_at\":1700172800}";
    }

    /// <summary>A text request for <c>gpt-5.6</c> in the shape <c>startBatch</c> normalizes it to.</summary>
    public static NormalizedBatchRequest TextRequest(string id, string prompt, Action<JsonObject>? configure = null, string modelId = "gpt-5.6")
    {
        var options = new JsonObject
        {
            ["prompt"] = new JsonArray(new JsonObject
            {
                ["role"] = "user",
                ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = prompt }),
            }),
        };
        configure?.Invoke(options);
        return new NormalizedBatchRequest(id, "text", modelId, System.Text.Json.JsonDocument.Parse(options.ToJsonString()).RootElement.Clone());
    }

    public static IBatchApi Batch(HttpMessageHandler handler, Action<OpenAIOptions>? configure = null)
    {
        var options = new OpenAIOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return OpenAIProvider.Create(options, handler).ExperimentalBatch()!;
    }

    public static BatchOperationCall Call(string? batchId = null, IReadOnlyList<NormalizedBatchRequest>? requests = null, IReadOnlyDictionary<string, string>? headers = null, string? providerOptions = null, string? webhookUrl = null, int? limit = null, string? cursor = null)
    {
        System.Text.Json.JsonElement? options = providerOptions == null ? null : System.Text.Json.JsonDocument.Parse(providerOptions).RootElement.Clone();
        return new BatchOperationCall(batchId, options, headers ?? new Dictionary<string, string>(), CancellationToken.None, limit, cursor, webhookUrl, requests);
    }

    /// <summary>
    /// Runs <paramref name="operation"/> once per request. Each run cancels its token when that request arrives and
    /// checks that the request saw the cancellation, which shows the token reached every request.
    /// </summary>
    public static async Task AssertForwardsCancellation(int requestCount, Func<RecordingHandler> handler, Func<IBatchApi, CancellationToken, Task> operation)
    {
        for (var request = 1; request <= requestCount; request++)
        {
            using var source = new CancellationTokenSource();
            var recording = handler();
            recording.CancelOn(request, source);
            await Assert.ThrowsAnyAsync<Exception>(() => operation(Batch(recording), source.Token));
            Assert.Equal(request, recording.Requests.Count);
            Assert.True(recording.CancellationObserved);
        }
    }

    public static JsonObject ResponsesResultBody(string text)
    {
        return new JsonObject
        {
            ["id"] = "resp_123",
            ["created_at"] = 1_700_000_000,
            ["model"] = "gpt-5.6",
            ["output"] = new JsonArray(MessageOutput(text)),
            ["service_tier"] = "default",
            ["reasoning"] = null,
            ["incomplete_details"] = null,
            ["usage"] = new JsonObject
            {
                ["input_tokens"] = 10,
                ["input_tokens_details"] = new JsonObject { ["cached_tokens"] = 2 },
                ["output_tokens"] = 3,
                ["output_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 1 },
            },
        };
    }

    public static JsonObject MessageOutput(string text)
    {
        return new JsonObject
        {
            ["type"] = "message",
            ["role"] = "assistant",
            ["id"] = "msg_123",
            ["phase"] = null,
            ["content"] = new JsonArray(new JsonObject
            {
                ["type"] = "output_text",
                ["text"] = text,
                ["logprobs"] = null,
                ["annotations"] = new JsonArray(),
            }),
        };
    }

    public static JsonObject WithOutput(JsonObject body, params JsonNode[] output)
    {
        body["output"] = new JsonArray(output);
        return body;
    }

    public static string ResultLine(string id, JsonNode body)
    {
        return new JsonObject
        {
            ["custom_id"] = id,
            ["response"] = new JsonObject { ["status_code"] = 200, ["request_id"] = "openai-" + id, ["body"] = body },
            ["error"] = null,
        }.ToJsonString();
    }

    public static async Task<IReadOnlyList<BatchItem>> GetResultsAsync(HttpMessageHandler handler)
    {
        var batch = OpenAIProvider.Create(new OpenAIOptions { ApiKey = "test-api-key" }, handler).ExperimentalBatch()!;
        var stream = await batch.DoGetResultsAsync(new BatchOperationCall("batch_123", null, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None);
        return await stream.ToListAsync();
    }
}

/// <summary>Serves JSON routes and records each request with its headers, body, and multipart fields.</summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _bodies = new(StringComparer.Ordinal);
    private int _cancelOn;
    private CancellationTokenSource? _cancel;

    public List<RecordedRequest> Requests { get; } = new();

    public bool CancellationObserved { get; private set; }

    public RecordingHandler Add(string url, string body)
    {
        _bodies[url] = body;
        return this;
    }

    public void CancelOn(int request, CancellationTokenSource source)
    {
        _cancelOn = request;
        _cancel = source;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = new RecordedRequest(request.Method.Method, request.RequestUri!.AbsoluteUri);
        foreach (var header in request.Headers.Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>()))
        {
            recorded.Headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        if (request.Content is MultipartFormDataContent multipart)
        {
            foreach (var part in multipart)
            {
                var disposition = part.Headers.ContentDisposition!;
                recorded.Parts[disposition.Name!.Trim('"')] = new RecordedPart(disposition.FileName?.Trim('"'), part.Headers.ContentType?.MediaType, await part.ReadAsStringAsync(cancellationToken));
            }
        }
        else if (request.Content != null)
        {
            recorded.Body = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        Requests.Add(recorded);
        if (Requests.Count == _cancelOn)
        {
            _cancel!.Cancel();
            CancellationObserved = cancellationToken.IsCancellationRequested;
            throw new OperationCanceledException(cancellationToken);
        }

        var url = request.RequestUri.GetLeftPart(UriPartial.Path);
        if (!_bodies.TryGetValue(url, out var body))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent("{\"error\":{\"message\":\"no route for " + url + "\"}}") };
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
    }
}

internal sealed class RecordedRequest
{
    public RecordedRequest(string method, string url)
    {
        Method = method;
        Url = url;
    }

    public string Method { get; }

    public string Url { get; }

    public Dictionary<string, string> Headers { get; } = new(StringComparer.Ordinal);

    public string? Body { get; set; }

    public Dictionary<string, RecordedPart> Parts { get; } = new(StringComparer.Ordinal);
}

internal sealed record RecordedPart(string? FileName, string? MediaType, string Text);

/// <summary>Serves each route as separate body chunks and records the request URLs in order.</summary>
internal sealed class ChunkedHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string[]> _chunks = new(StringComparer.Ordinal);

    public List<string> RequestUrls { get; } = new();

    public ChunkedHandler Add(string url, params string[] chunks)
    {
        _chunks[url] = chunks;
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.AbsoluteUri ?? string.Empty;
        RequestUrls.Add(url);
        if (!_chunks.TryGetValue(url, out var chunks))
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent("{\"error\":{\"message\":\"no route for " + url + "\"}}") });
        }

        var content = new StreamContent(new ChunkStream(chunks));
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/jsonl");
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content });
    }

    private sealed class ChunkStream : Stream
    {
        private readonly Queue<byte[]> _chunks;

        public ChunkStream(string[] chunks)
        {
            _chunks = new Queue<byte[]>(chunks.Select(chunk => System.Text.Encoding.UTF8.GetBytes(chunk)));
        }

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
            if (_chunks.Count == 0)
            {
                return 0;
            }

            var chunk = _chunks.Dequeue();
            chunk.CopyTo(buffer, offset);
            return chunk.Length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
