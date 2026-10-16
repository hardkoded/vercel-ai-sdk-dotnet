// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Mcp;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

/// <summary>Regression tests for cancellation and cleanup in transports and stream readers.</summary>
public sealed class TransportCancellationTests
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task SseParser_stops_waiting_for_the_next_line_when_the_caller_cancels()
    {
        using var source = new CancellationTokenSource();
        using var stream = new StalledStream("data: {\"a\":1}\n\n");
        var seen = new List<string>();
        var read = Task.Run(async () =>
        {
            await foreach (var data in SseParser.ReadDataAsync(stream, source.Token))
            {
                seen.Add(data);
                source.CancelAfter(50);
            }
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read.WaitAsync(Limit));
        Assert.Equal(new[] { "{\"a\":1}" }, seen);
    }

    [Fact]
    public async Task ReadJsonEventsAsync_stops_waiting_for_the_next_chunk_when_the_caller_cancels()
    {
        using var source = new CancellationTokenSource();
        using var stream = new StalledStream("data: {\"a\":1}\n\n");
        var read = Task.Run(async () =>
        {
            await foreach (var _ in JsonStreams.ReadJsonEventsAsync(stream, null, source.Token))
            {
                source.CancelAfter(50);
            }
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read.WaitAsync(Limit));
    }

    [Fact]
    public async Task ProviderHttp_stops_reading_a_stalled_body_when_the_caller_cancels()
    {
        using var source = new CancellationTokenSource();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new StalledStream(string.Empty)) });
        var http = new ProviderHttp(new HttpClient(handler), RetryPolicy.Default);
        var call = http.SendJsonAsync(HttpMethod.Post, new Uri("https://example.test/x"), "{}", null, source.Token);
        source.CancelAfter(50);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call.WaitAsync(Limit));
    }

    [Fact]
    public async Task ProviderHttp_disposes_the_response_after_a_json_read()
    {
        var content = new TrackingContent("{}");
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var http = new ProviderHttp(new HttpClient(handler), RetryPolicy.Default);

        using var document = await http.SendJsonAsync(HttpMethod.Post, new Uri("https://example.test/x"), "{}", null, CancellationToken.None);

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_disposes_the_response_when_the_body_read_fails()
    {
        var content = new TrackingContent(null);
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var provider = new OpenAICompatibleProvider(new OpenAICompatibleOptions { ApiKey = "secret", ProviderName = "p" }, new HttpClient(handler));

        await Assert.ThrowsAsync<HttpRequestException>(() => provider.GetBinaryAsync(new Uri("https://example.test/x"), null, CancellationToken.None));

        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task OpenAICompatible_stops_reading_a_stalled_body_when_the_caller_cancels()
    {
        using var source = new CancellationTokenSource();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new StalledStream(string.Empty)) });
        var provider = new OpenAICompatibleProvider(new OpenAICompatibleOptions { ApiKey = "secret", ProviderName = "p" }, new HttpClient(handler));
        var call = provider.GetBinaryAsync(new Uri("https://example.test/x"), null, source.Token);
        source.CancelAfter(50);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call.WaitAsync(Limit));
    }

    [Fact]
    public void ApiUserAbortException_is_an_OperationCanceledException_and_not_an_AiSdkException()
    {
        Exception error = new ApiUserAbortException();

        Assert.IsAssignableFrom<OperationCanceledException>(error);
        Assert.False(error is AiSdkException);
    }

    [Fact]
    public async Task ProviderExchange_surfaces_a_caller_cancel_as_ApiUserAbortException()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var error = await Record.ExceptionAsync(() => ProviderExchange.SendAsync(new HttpClient(handler), HttpMethod.Get, new Uri("https://example.test/x"), null, null, source.Token));

        Assert.IsType<ApiUserAbortException>(error);
    }

    [Fact]
    public async Task ProviderExchange_surfaces_a_cancel_during_the_body_read_as_ApiUserAbortException()
    {
        using var source = new CancellationTokenSource();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new StalledStream(string.Empty)) });
        var call = ProviderExchange.SendAsync(new HttpClient(handler), HttpMethod.Get, new Uri("https://example.test/x"), null, null, source.Token);
        source.CancelAfter(50);

        var error = await Record.ExceptionAsync(() => call.WaitAsync(Limit));

        Assert.IsType<ApiUserAbortException>(error);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProviderHttp_surfaces_a_cancel_during_the_retry_delay_as_ApiUserAbortException(bool connectionFailure)
    {
        using var source = new CancellationTokenSource();
        var handler = new DelegateHandler(_ => connectionFailure
            ? throw new HttpRequestException("down")
            : new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new ProviderHttp(new HttpClient(handler), new RetryPolicy { MaxRetries = 2, InitialDelay = TimeSpan.FromMinutes(1), MaxDelay = TimeSpan.FromMinutes(1), Jitter = 0 });
        var call = http.SendJsonAsync(HttpMethod.Post, new Uri("https://example.test/x"), "{}", null, source.Token);
        source.CancelAfter(100);

        var error = await Record.ExceptionAsync(() => call.WaitAsync(Limit));

        Assert.IsType<ApiUserAbortException>(error);
    }

    [Fact]
    public async Task Gateway_batch_results_let_a_caller_cancel_through()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) });
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "key", BaseUrl = "https://api.test.com" }, handler);
        var batch = provider.ExperimentalBatch()!;

        var error = await Record.ExceptionAsync(() => batch.DoGetResultsAsync(new BatchOperationCall("job_1", null, new Dictionary<string, string>(), CancellationToken.None), source.Token));

        Assert.IsType<Vercel.AI.Provider.ApiUserAbortException>(error);
    }

    [Fact]
    public async Task AsyncIterableStream_enumeration_honors_the_cancellation_token()
    {
        using var source = new CancellationTokenSource();
        var stream = new AsyncIterableStream<int>(new ReadableStream<int>(pull: _ => Task.Delay(Timeout.Infinite)));
        var read = Task.Run(async () =>
        {
            await foreach (var _ in stream.WithCancellation(source.Token))
            {
            }
        });
        source.CancelAfter(50);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read.WaitAsync(Limit));
    }

    [Fact]
    public async Task ReadableStream_fails_the_read_when_the_pull_throws_synchronously()
    {
        var stream = new ReadableStream<int>(pull: new Func<ReadableStreamController<int>, Task>(_ => throw new InvalidOperationException("boom")));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => stream.ToArrayAsync().WaitAsync(Limit));

        Assert.Equal("boom", error.Message);
    }

    [Fact]
    public async Task ReadableStream_fails_the_read_when_the_pull_is_cancelled()
    {
        var pulls = 0;
        var stream = new ReadableStream<int>(pull: new Func<ReadableStreamController<int>, Task>(_ =>
        {
            Interlocked.Increment(ref pulls);
            return Task.FromCanceled(new CancellationToken(true));
        }));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.ToArrayAsync().WaitAsync(Limit));

        Assert.Equal(1, pulls);
    }

    [Fact]
    public void AbortSignal_runs_every_handler_when_one_throws()
    {
        var signal = new AbortSignal();
        var ran = new List<int>();
        signal.AddAbortHandler(() => ran.Add(1));
        signal.AddAbortHandler(() => throw new InvalidOperationException("boom"));
        signal.AddAbortHandler(() => ran.Add(3));

        Assert.Throws<InvalidOperationException>(() => signal.Abort("reason"));

        Assert.Equal(new[] { 1, 3 }, ran);
        Assert.True(signal.Aborted);
    }

    [Fact]
    public async Task AbortSignal_runs_each_handler_once_when_aborted_concurrently()
    {
        var signal = new AbortSignal();
        var runs = 0;
        signal.AddAbortHandler(() => Interlocked.Increment(ref runs));

        await Task.WhenAll(Enumerable.Range(0, 16).Select(i => Task.Run(() => signal.Abort(i))));

        Assert.Equal(1, runs);
    }

    [Fact]
    public void MergeAbortSignals_stops_listening_to_the_sources_once_the_merged_signal_aborts()
    {
        var first = new AbortController();
        var second = new AbortController();
        var merged = MergeAbortSignals.Merge(first.Signal, second.Signal)!;

        Assert.Equal(1, first.Signal.HandlerCount);
        first.Abort("stop");

        Assert.True(merged.Aborted);
        Assert.Equal(0, second.Signal.HandlerCount);
    }

    [Fact]
    public async Task HttpMcpTransport_skips_notifications_and_returns_the_response_for_the_request_id()
    {
        var body = "data: {\"jsonrpc\":\"2.0\",\"method\":\"notifications/progress\",\"params\":{}}\n\n"
            + "data: {\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"ok\":true}}\n\n";
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "text/event-stream") });
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");

        var result = await transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None);

        Assert.True(result.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task HttpMcpTransport_returns_before_an_open_sse_stream_ends_and_honors_cancellation()
    {
        using var source = new CancellationTokenSource();
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StalledStream("data: {\"jsonrpc\":\"2.0\",\"method\":\"notifications/progress\"}\n\n"))
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream") },
            },
        });
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");
        var call = transport.CallAsync("tools/list", parameters.RootElement, source.Token);
        source.CancelAfter(100);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call.WaitAsync(Limit));
    }

    [Fact]
    public async Task HttpMcpTransport_fails_when_the_sse_stream_ends_without_a_response()
    {
        var handler = new DelegateHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data: {\"jsonrpc\":\"2.0\",\"method\":\"notifications/progress\"}\n\n", Encoding.UTF8, "text/event-stream"),
        });
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");

        await Assert.ThrowsAsync<MCPClientError>(() => transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None));
    }

    [Fact]
    public async Task McpNdjson_writes_without_blocking_and_honors_the_token()
    {
        using var document = JsonDocument.Parse("{\"id\":1}");
        using var output = new MemoryStream();

        await McpNdjson.WriteAsync(output, document.RootElement, CancellationToken.None);

        Assert.Equal("{\"id\":1}\n", Encoding.UTF8.GetString(output.ToArray()));
        await Assert.ThrowsAsync<MCPClientError>(() => McpNdjson.WriteAsync(null, document.RootElement, CancellationToken.None));
    }

    /// <summary>Yields a prefix, then waits for the token like a socket whose server went quiet.</summary>
    private sealed class StalledStream(string prefix) : Stream
    {
        private readonly byte[] _prefix = Encoding.UTF8.GetBytes(prefix);
        private int _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_position < _prefix.Length)
            {
                var count = Math.Min(buffer.Length, _prefix.Length - _position);
                _prefix.AsMemory(_position, count).CopyTo(buffer);
                _position += count;
                return count;
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Content that records its disposal. A null body makes the read throw.</summary>
    private sealed class TrackingContent(string? body) : HttpContent
    {
        public bool Disposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            if (body is null)
            {
                throw new IOException("connection reset");
            }

            var bytes = Encoding.UTF8.GetBytes(body);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(respond(request));
        }
    }
}
