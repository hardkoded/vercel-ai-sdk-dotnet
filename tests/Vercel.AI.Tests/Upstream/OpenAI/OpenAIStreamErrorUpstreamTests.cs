// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Threading.Channels;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>OpenAI error bodies, stream error classification, and the early-error check before output.</summary>
public sealed class OpenAIStreamErrorUpstreamTests
{
    private const string Early = "packages/openai/src/openai-stream-error.test.ts::throwIfOpenAIStreamErrorBeforeOutput::";
    private const string Accepted = "packages/openai/src/openai-stream-error.test.ts::throwIfOpenAIStreamErrorBeforeOutput > with an accepted-chunk detector::";
    private const string Classify = "packages/openai/src/openai-stream-error.test.ts::createOpenAIProviderStreamError::";

    [Fact]
    [UpstreamTest("packages/openai/src/openai-error.test.ts::openaiErrorDataSchema::should parse OpenRouter resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsesOpenRouterResourceExhaustedError()
    {
        const string message = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";
        var capture = new OpenAICapture
        {
            Status = (HttpStatusCode)429,
            ResponseJson = JsonSerializer.Serialize(new { error = new { message, code = 429 } }),
        };
        var model = OpenAIUpstream.Provider(capture).ChatModel("gpt-4o-mini");
        var error = await Assert.ThrowsAsync<RateLimitException>(() => model.DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal(message, error.Message);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Early + "should throw when an error frame arrives before output without cancelling the source", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsOnErrorBeforeOutputWithoutDisposingSource()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("error", "quota exceeded"));
        var error = await Assert.ThrowsAsync<InternalServerException>(() => Check(source)).ConfigureAwait(false);
        Assert.Contains("quota exceeded", error.ResponseBody, StringComparison.Ordinal);
        Assert.False(source.Disposed);
    }

    [Fact]
    [UpstreamTest(Early + "should resolve on the first output chunk and replay all chunks to the consumer", Coverage = UpstreamCoverage.Covered)]
    public async Task ResolvesOnFirstOutputAndReplays()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("output", "hello"));
        source.Close();
        var chunks = await ReadAll(await Check(source).ConfigureAwait(false)).ConfigureAwait(false);
        Assert.Equal(new[] { new Chunk("created"), new Chunk("output", "hello") }, chunks);
    }

    [Fact]
    [UpstreamTest(Early + "should keep blocking until output when no accepted-chunk detector is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task BlocksUntilOutputWithoutAcceptedDetector()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("accepted"));
        var pending = Check(source);
        await Task.Delay(30).ConfigureAwait(false);
        Assert.False(pending.IsCompleted);

        source.Enqueue(new Chunk("output", "late"));
        source.Close();
        var chunks = await ReadAll(await pending.ConfigureAwait(false)).ConfigureAwait(false);
        Assert.Equal(new[] { new Chunk("created"), new Chunk("accepted"), new Chunk("output", "late") }, chunks);
    }

    [Fact]
    [UpstreamTest(Accepted + "should resolve after the grace window when the stream stalls after acceptance", Coverage = UpstreamCoverage.Covered)]
    public async Task ResolvesAfterGraceWindowWhenStalled()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("accepted"));
        var checkedStream = await Check(source, acceptedGraceMs: 10).ConfigureAwait(false);

        source.Enqueue(new Chunk("output", "late token"));
        source.Close();
        var chunks = await ReadAll(checkedStream).ConfigureAwait(false);
        Assert.Equal(new[] { new Chunk("created"), new Chunk("accepted"), new Chunk("output", "late token") }, chunks);
    }

    [Fact]
    [UpstreamTest(Accepted + "should still throw when an error frame is flushed together with the accepted chunk", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsWhenErrorFollowsAcceptedChunk()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("accepted"));
        source.Enqueue(new Chunk("error", "insufficient quota"));
        source.Close();
        var error = await Assert.ThrowsAsync<InternalServerException>(() => Check(source, acceptedGraceMs: 10)).ConfigureAwait(false);
        Assert.Contains("insufficient quota", error.ResponseBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Accepted + "should surface a source error to the consumer after grace resolution without unhandled rejections", Coverage = UpstreamCoverage.Covered)]
    public async Task SurfacesSourceErrorAfterGraceResolution()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("accepted"));
        var checkedStream = await Check(source, acceptedGraceMs: 10).ConfigureAwait(false);

        source.Error(new IOException("connection reset"));
        var error = await Assert.ThrowsAsync<IOException>(() => ReadAll(checkedStream)).ConfigureAwait(false);
        Assert.Equal("connection reset", error.Message);
    }

    [Fact]
    [UpstreamTest(Accepted + "should resolve immediately on output without waiting for the grace window", Coverage = UpstreamCoverage.Covered)]
    public async Task ResolvesImmediatelyOnOutput()
    {
        var source = new ControlledStream();
        source.Enqueue(new Chunk("created"));
        source.Enqueue(new Chunk("accepted"));
        source.Enqueue(new Chunk("output", "hello"));
        source.Close();
        var check = Check(source, acceptedGraceMs: 60_000);
        Assert.Same(check, await Task.WhenAny(check, Task.Delay(5_000)).ConfigureAwait(false));
        var chunks = await ReadAll(await check.ConfigureAwait(false)).ConfigureAwait(false);
        Assert.Equal(new[] { new Chunk("created"), new Chunk("accepted"), new Chunk("output", "hello") }, chunks);
    }

    [Fact]
    [UpstreamTest(Classify + "classifies a documented top-level rate-limit event", Coverage = UpstreamCoverage.Covered)]
    public void ClassifiesTopLevelRateLimit()
    {
        var data = OpenAIUpstream.Json("{\"type\":\"error\",\"code\":\"rate_limit_exceeded\",\"message\":\"Rate limit reached\",\"param\":null}");
        AssertError(data, "Rate limit reached", "error", "rate_limit_exceeded", 429, true);
    }

    [Fact]
    [UpstreamTest(Classify + "classifies insufficient quota as non-retryable", Coverage = UpstreamCoverage.Covered)]
    public void ClassifiesInsufficientQuotaAsNonRetryable()
    {
        var data = OpenAIUpstream.Json("{\"type\":\"error\",\"code\":\"insufficient_quota\",\"message\":\"You exceeded your current quota.\",\"param\":null}");
        AssertError(data, "You exceeded your current quota.", "error", "insufficient_quota", 429, false);
    }

    [Fact]
    [UpstreamTest(Classify + "preserves the provider type when code is an HTTP status", Coverage = UpstreamCoverage.Covered)]
    public void PreservesTypeWhenCodeIsHttpStatus()
    {
        var data = OpenAIUpstream.Json("{\"type\":\"rate_limit_error\",\"code\":\"429\",\"message\":\"Rate limit reached\"}");
        AssertError(data, "Rate limit reached", "rate_limit_error", "429", 429, true);
    }

    [Fact]
    [UpstreamTest(Classify + "classifies a response.failed server error by its provider code", Coverage = UpstreamCoverage.Covered)]
    public void ClassifiesResponseFailedServerError()
    {
        var data = OpenAIUpstream.Json("{\"type\":\"response.failed\",\"response\":{\"error\":{\"code\":\"server_error\",\"message\":\"Response failed\"}}}");
        AssertError(data, "Response failed", "response.failed", "server_error", 500, true);
    }

    private static void AssertError(JsonElement data, string message, string type, string code, int statusCode, bool isRetryable)
    {
        var error = OpenAIStreamError.CreateProviderStreamError(data);
        Assert.NotNull(error);
        Assert.Equal(message, error!.Message);
        Assert.Equal(type, error.Type);
        Assert.Equal(code, error.Code);
        Assert.Equal(statusCode, error.StatusCode);
        Assert.Equal(isRetryable, error.IsRetryable);
        Assert.Equal(data.GetRawText(), ((JsonElement)error.Data!).GetRawText());
    }

    private static Task<IAsyncEnumerable<Chunk>> Check(ControlledStream source, int? acceptedGraceMs = null)
    {
        return acceptedGraceMs is int grace
            ? OpenAIStreamError.ThrowIfErrorBeforeOutputAsync<Chunk>(source, GetError, IsOutput, chunk => chunk.Type == "accepted", grace)
            : OpenAIStreamError.ThrowIfErrorBeforeOutputAsync<Chunk>(source, GetError, IsOutput);
    }

    private static JsonElement? GetError(Chunk chunk)
    {
        return chunk.Type == "error" ? JsonSerializer.SerializeToElement(new { error = new { type = chunk.Type, message = chunk.Text } }) : null;
    }

    private static bool IsOutput(Chunk chunk)
    {
        return chunk.Type == "output";
    }

    private static async Task<List<Chunk>> ReadAll(IAsyncEnumerable<Chunk> stream)
    {
        var chunks = new List<Chunk>();
        await foreach (var chunk in stream.ConfigureAwait(false))
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private sealed record Chunk(string Type, string? Text = null);

    /// <summary>A source the test feeds by hand. It records whether it was disposed.</summary>
    private sealed class ControlledStream : IAsyncEnumerator<Chunk>
    {
        private readonly Channel<Chunk> _channel = Channel.CreateUnbounded<Chunk>();

        public bool Disposed { get; private set; }

        public Chunk Current { get; private set; } = new Chunk(string.Empty);

        public void Enqueue(Chunk chunk) => _channel.Writer.TryWrite(chunk);

        public void Close() => _channel.Writer.TryComplete();

        public void Error(Exception error) => _channel.Writer.TryComplete(error);

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                return false;
            }

            Current = await _channel.Reader.ReadAsync().ConfigureAwait(false);
            return true;
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return default;
        }
    }
}
