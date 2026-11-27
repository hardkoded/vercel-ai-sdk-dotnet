// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>A test server for <c>https://api.test.com/language-model</c>. It records each request and answers with a canned response.</summary>
internal sealed class GatewayLanguageModelServer : HttpMessageHandler
{
    public string ResponseBody { get; set; } = string.Empty;

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string ContentType { get; set; } = "text/event-stream";

    /// <summary>When set, the request throws this exception, as a failing <c>fetch</c> does.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Runs while the request is handled, before the response is returned.</summary>
    public Action? BeforeRespond { get; set; }

    /// <summary>When set, the body is read in these chunks, as upstream <c>stream-chunks</c>.</summary>
    public IReadOnlyList<string>? ResponseChunks { get; set; }

    public int Calls { get; private set; }

    public string RequestUrl { get; private set; } = string.Empty;

    public string RequestBody { get; private set; } = string.Empty;

    public Dictionary<string, string> RequestHeaders { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public CancellationToken RequestToken { get; private set; }

    public JsonNode RequestJson => JsonNode.Parse(RequestBody)!;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Calls++;
        RequestUrl = request.RequestUri?.AbsoluteUri ?? string.Empty;
        RequestToken = cancellationToken;
        RequestBody = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        foreach (var header in request.Headers)
        {
            RequestHeaders[header.Key] = string.Join(",", header.Value);
        }

        BeforeRespond?.Invoke();
        cancellationToken.ThrowIfCancellationRequested();
        if (Failure != null)
        {
            throw Failure;
        }

        HttpContent content = ResponseChunks is null
            ? new StringContent(ResponseBody, Encoding.UTF8)
            : new StreamContent(new ChunkedStream(ResponseChunks));
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
        return new HttpResponseMessage(Status) { Content = content };
    }

    /// <summary>A read-only stream that returns one chunk per read.</summary>
    private sealed class ChunkedStream(IReadOnlyList<string> chunks) : Stream
    {
        private int _index;
        private byte[] _pending = [];
        private int _offset;

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
            if (_offset >= _pending.Length)
            {
                if (_index >= chunks.Count)
                {
                    return 0;
                }

                _pending = Encoding.UTF8.GetBytes(chunks[_index++]);
                _offset = 0;
            }

            var length = Math.Min(count, _pending.Length - _offset);
            Array.Copy(_pending, _offset, buffer, offset, length);
            _offset += length;
            return length;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}

/// <summary>Helpers shared by the <c>gateway-language-model.test.ts</c> doStream ports.</summary>
internal static class GatewayLanguageModelSupport
{
    internal static readonly LanguageModelCallOptions TestPrompt = new()
    {
        Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
    };

    internal static ILanguageModel CreateTestModel(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    ) =>
        new Vercel.AI.Gateway.GatewayLanguageModel(
            GatewayProvider.Create(
                new GatewayOptions
                {
                    ApiKey = "test-token",
                    BaseUrl = "https://api.test.com",
                    O11yHeaders = o11yHeaders ?? new Dictionary<string, string>(),
                },
                handler
            ),
            "test-model",
            "test-provider"
        );

    /// <summary>
    /// Builds the model with a <c>fetch</c> hook, as upstream <c>createTestModel({ fetch })</c>. The hook receives the token the
    /// send uses. <see cref="HttpClient"/> would wrap a handler exception and always add a token, so the hook replaces it.
    /// </summary>
    internal static ILanguageModel CreateTestModel(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> fetch
    )
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = new Dictionary<string, string>(),
            },
            new GatewayLanguageModelServer()
        );
        provider.Http.Fetch = fetch;
        return new Vercel.AI.Gateway.GatewayLanguageModel(provider, "test-model", "test-provider");
    }

    /// <summary>A fetch that sends through <paramref name="server"/> and records the token it was given, as <c>vi.fn().mockImplementation(globalThis.fetch)</c>.</summary>
    internal static Func<
        HttpRequestMessage,
        CancellationToken,
        Task<HttpResponseMessage>
    > Recording(GatewayLanguageModelServer server, List<CancellationToken> tokens)
    {
        var invoker = new HttpMessageInvoker(server);
        return (request, token) =>
        {
            tokens.Add(token);
            return invoker.SendAsync(request, token);
        };
    }

    /// <summary>Builds the server response of upstream <c>prepareStreamResponse</c>.</summary>
    internal static GatewayLanguageModelServer PrepareStreamResponse(params string[] content)
    {
        var body = new StringBuilder();
        foreach (var text in content)
        {
            body.Append("data: {\"type\":\"text-delta\",\"textDelta\":\"")
                .Append(text)
                .Append("\"}\n\n");
        }

        body.Append(
            "data: {\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}\n\n"
        );
        return new GatewayLanguageModelServer { ResponseBody = body.ToString() };
    }

    /// <summary>
    /// Builds the server response of upstream <c>prepareJsonResponse</c>. The port reads typed results, so the content is the
    /// array the Gateway returns and the usage uses the V4 token counts.
    /// </summary>
    internal static GatewayLanguageModelServer PrepareJsonResponse(
        string text = "",
        string? warnings = null,
        string usage = "{\"inputTokens\":{\"total\":4},\"outputTokens\":{\"total\":30}}"
    ) =>
        new()
        {
            ContentType = "application/json",
            ResponseBody =
                "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":[{\"type\":\"text\",\"text\":"
                + System.Text.Json.JsonSerializer.Serialize(text)
                + "}],\"finish_reason\":\"stop\",\"usage\":"
                + usage
                + (warnings is null ? string.Empty : ",\"warnings\":" + warnings)
                + "}",
        };

    internal static GatewayLanguageModelServer ErrorResponse(int status, string body) =>
        new()
        {
            Status = (HttpStatusCode)status,
            ContentType = "application/json",
            ResponseBody = body,
        };

    internal static GatewayLanguageModelServer StreamChunks(params string[] chunks) =>
        new() { ResponseBody = string.Concat(chunks.Select(chunk => "data: " + chunk + "\n\n")) };

    internal static async Task<List<LanguageModelStreamPart>> ReadAll(
        LanguageModelStreamResult result
    )
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        return parts;
    }
}
