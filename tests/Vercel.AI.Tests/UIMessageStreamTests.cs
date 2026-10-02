// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Http;
using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class UIMessageStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream::maps text and lifecycle parts to UI message chunks",
        Coverage = UpstreamCoverage.Partial,
        Note = "Writes text, tool, source, and finish SSE chunks, including [DONE].")]
    public async Task Writes_text_tool_and_finish_chunks()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "Hi"),
                new ToolCallStreamPart("call_1", "lookup", "{\"q\":1}"),
                new SourceStreamPart("src_1", "https://example.test/doc", "Doc"),
                new FinishStreamPart(FinishReason.ToolCalls, new LanguageModelUsage(1, 1, 2), "tool_calls"),
            },
        };
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var stream = client.StreamTextAsync(new StreamTextOptions
        {
            Model = model,
            Prompt = "hi",
            Tools = new[] { Tool.Function("lookup", "find", "{\"type\":\"object\"}", (_, _) => Task.FromResult("{\"n\":1}")) },
        });
        using var buffer = new MemoryStream();
        await UIMessageStreamResult.WriteAsync(stream, buffer);
        var text = System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        Assert.Contains("\"type\":\"start\"", text);
        Assert.Contains("\"type\":\"text-start\"", text);
        Assert.Contains("\"type\":\"text-delta\"", text);
        Assert.Contains("\"delta\":\"Hi\"", text);
        Assert.Contains("\"type\":\"text-end\"", text);
        Assert.Contains("\"type\":\"tool-input-available\"", text);
        Assert.Contains("\"type\":\"tool-output-available\"", text);
        Assert.Contains("\"type\":\"source-url\"", text);
        Assert.Contains("\"type\":\"finish-step\"", text);
        Assert.Contains("\"type\":\"finish\"", text);
        Assert.Contains("data: [DONE]", text);
        AssertNoCommentLines(text);
    }

    [Fact]
    public async Task ShouldPassThroughTheOriginalStreamWhenKeepAliveMsIsUndefined()
    {
        var source = new ChunkList("data");
        Assert.Same(source, UIMessageStreamResult.CreateSseStreamWithKeepAlive(source, null));

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var immediate = client.StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new TextDeltaStreamPart("text", "Hi"),
                    new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
                },
            },
            Prompt = "hi",
        });
        using var plain = new MemoryStream();
        await UIMessageStreamResult.WriteAsync(immediate, plain);
        AssertNoCommentLines(Encoding.UTF8.GetString(plain.ToArray()));
    }

    [Fact]
    public async Task ShouldSendAnOpeningCommentImmediatelyAndCommentsWhileTheSourceIsIdle()
    {
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var idle = client.StreamTextAsync(new StreamTextOptions
        {
            Model = new IdleLanguageModel(release.Task),
            Prompt = "hi",
        });
        using var buffer = new SnapshotStream();
        var writing = UIMessageStreamResult.WriteAsync(idle, buffer, keepAliveMs: 40);
        try
        {
            var sawKeepAlive = false;
            for (var attempt = 0; attempt < 200 && !sawKeepAlive; attempt++)
            {
                sawKeepAlive = buffer.Text.Contains(": keep-alive\n\n", StringComparison.Ordinal);
                if (!sawKeepAlive)
                {
                    await Task.Delay(15);
                }
            }

            Assert.True(sawKeepAlive);
        }
        finally
        {
            release.TrySetResult(true);
        }

        await writing.WaitAsync(TimeSpan.FromSeconds(5));
        var text = buffer.Text;
        var openAt = text.IndexOf(": stream-open\n\n", StringComparison.Ordinal);
        var dataAt = text.IndexOf("data:", StringComparison.Ordinal);
        var keepAliveAt = text.IndexOf(": keep-alive\n\n", StringComparison.Ordinal);
        var partAt = text.IndexOf("\"delta\":\"Later\"", StringComparison.Ordinal);
        Assert.True(openAt >= 0 && dataAt > openAt);
        Assert.True(keepAliveAt >= 0 && partAt > keepAliveAt);
    }

    [Fact]
    public async Task ShouldRetainOnePendingSourceReadAcrossManyIdleKeepAlives()
    {
        var source = new PendingSource();
        var enumerator = UIMessageStreamResult.CreateSseStreamWithKeepAlive(source, 1).GetAsyncEnumerator();
        try
        {
            Assert.Equal(": stream-open\n\n", await ReadChunk(enumerator));
            for (var i = 0; i < 2500; i++)
            {
                Assert.Equal(": keep-alive\n\n", await ReadChunk(enumerator));
            }

            Assert.Equal(1, source.Pulls);
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        Assert.Equal(1, source.Cancels);
    }

    [Fact]
    public async Task ShouldResetTheKeepAliveTimerAfterSourceActivity()
    {
        var source = new GatedSource();
        var enumerator = UIMessageStreamResult.CreateSseStreamWithKeepAlive(source, 100).GetAsyncEnumerator();
        try
        {
            Assert.Equal(": stream-open\n\n", await ReadChunk(enumerator));
            var next = ReadChunk(enumerator);
            await Task.Delay(50);
            Assert.False(next.IsCompleted);
            source.Enqueue("data");
            Assert.Equal("data", await next.WaitAsync(TimeSpan.FromSeconds(2)));

            var started = Stopwatch.StartNew();
            var keepAlive = ReadChunk(enumerator);
            await Task.Delay(80);
            Assert.False(keepAlive.IsCompleted);
            Assert.Equal(": keep-alive\n\n", await keepAlive.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.InRange(started.ElapsedMilliseconds, 90, 2000);
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    [Fact]
    public async Task ShouldPreserveSourceChunksAndCompletion()
    {
        var actual = new List<string>();
        await foreach (var chunk in UIMessageStreamResult.CreateSseStreamWithKeepAlive(new ChunkList("data 1", "data 2"), 100))
        {
            actual.Add(chunk);
        }

        Assert.Equal(new[] { ": stream-open\n\n", "data 1", "data 2" }, actual);

        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var stream = client.StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel
            {
                StreamParts = new LanguageModelStreamPart[]
                {
                    new TextDeltaStreamPart("text", "Hi"),
                    new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop"),
                },
            },
            Prompt = "hi",
        });
        var httpContext = new DefaultHttpContext();
        using var body = new MemoryStream();
        httpContext.Response.Body = body;
        await stream.ToUIMessageStreamResult(60_000).ExecuteAsync(httpContext);
        var text = Encoding.UTF8.GetString(body.ToArray());
        var openAt = text.IndexOf(": stream-open\n\n", StringComparison.Ordinal);
        var dataAt = text.IndexOf("data:", StringComparison.Ordinal);
        Assert.True(openAt >= 0 && dataAt > openAt);
        Assert.DoesNotContain(": keep-alive", text, StringComparison.Ordinal);
        Assert.Equal("text/event-stream", httpContext.Response.ContentType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Rejects_keep_alive_that_is_not_a_positive_duration(int keepAliveMs)
    {
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var stream = client.StreamTextAsync(new StreamTextOptions
        {
            Model = new TestLanguageModel(),
            Prompt = "hi",
        });
        var fromResult = Assert.Throws<ArgumentOutOfRangeException>(() => stream.ToUIMessageStreamResult(keepAliveMs));
        Assert.Equal("keepAliveMs", fromResult.ParamName);
        using var buffer = new MemoryStream();
        var fromWrite = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => UIMessageStreamResult.WriteAsync(stream, buffer, keepAliveMs));
        Assert.Equal("keepAliveMs", fromWrite.ParamName);
        Assert.NotNull(stream.ToUIMessageStreamResult(int.MaxValue));
    }

    [Fact]
    public async Task ShouldCancelTheSourceStream()
    {
        var source = new PendingSource();
        var enumerator = UIMessageStreamResult.CreateSseStreamWithKeepAlive(source, 100).GetAsyncEnumerator();
        Assert.Equal(": stream-open\n\n", await ReadChunk(enumerator));
        await enumerator.DisposeAsync();
        Assert.Equal(1, source.Cancels);
        Assert.Equal(0, source.Pulls);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        using var cts = new CancellationTokenSource();
        var stream = client.StreamTextAsync(
            new StreamTextOptions { Model = new IdleLanguageModel(release.Task), Prompt = "hi" },
            cts.Token);
        using var buffer = new MemoryStream();
        var writing = UIMessageStreamResult.WriteAsync(stream, buffer, keepAliveMs: 40, cts.Token);
        cts.CancelAfter(TimeSpan.FromMilliseconds(80));
        try
        {
            var finished = await Task.WhenAny(writing, Task.Delay(TimeSpan.FromSeconds(3)));
            Assert.Same(writing, finished);
            var exception = await Record.ExceptionAsync(() => writing);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }
        finally
        {
            release.TrySetResult(true);
        }
    }

    private static async Task<string> ReadChunk(IAsyncEnumerator<string> enumerator)
    {
        Assert.True(await enumerator.MoveNextAsync());
        return enumerator.Current;
    }

    private static void AssertNoCommentLines(string text)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            Assert.True(line.Length == 0 || line[0] != ':');
        }
    }

    private sealed class IdleLanguageModel : ILanguageModel
    {
        private readonly Task _release;

        public IdleLanguageModel(Task release)
        {
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "idle";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            return Task.FromResult(TestLanguageModel.Text("ok"));
        }

        public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
            LanguageModelCallOptions options,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await _release.WaitAsync(cancellationToken).ConfigureAwait(false);
            yield return new TextDeltaStreamPart("text", "Later");
            yield return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 1, 2), "stop");
        }
    }

    private sealed class SnapshotStream : Stream
    {
        private readonly MemoryStream _buffer = new();
        private readonly object _gate = new();

        public string Text
        {
            get
            {
                lock (_gate)
                {
                    return Encoding.UTF8.GetString(_buffer.ToArray());
                }
            }
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

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
            lock (_gate)
            {
                _buffer.Write(buffer, offset, count);
            }
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            lock (_gate)
            {
                _buffer.Write(buffer);
            }
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Write(buffer.Span);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ChunkList : IAsyncEnumerable<string>
    {
        private readonly string[] _chunks;

        public ChunkList(params string[] chunks)
        {
            _chunks = chunks;
        }

        public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(_chunks);
        }

        private sealed class Enumerator : IAsyncEnumerator<string>
        {
            private readonly string[] _chunks;
            private int _index = -1;

            public Enumerator(string[] chunks)
            {
                _chunks = chunks;
            }

            public string Current => _chunks[_index];

            public ValueTask<bool> MoveNextAsync()
            {
                _index++;
                return new ValueTask<bool>(_index < _chunks.Length);
            }

            public ValueTask DisposeAsync()
            {
                return default;
            }
        }
    }

    private sealed class PendingSource : IAsyncEnumerable<string>
    {
        private readonly TaskCompletionSource<bool> _never = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _pulls;
        private int _cancels;

        public int Pulls => Volatile.Read(ref _pulls);

        public int Cancels => Volatile.Read(ref _cancels);

        public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this);
        }

        private sealed class Enumerator : IAsyncEnumerator<string>
        {
            private readonly PendingSource _owner;

            public Enumerator(PendingSource owner)
            {
                _owner = owner;
            }

            public string Current => string.Empty;

            public ValueTask<bool> MoveNextAsync()
            {
                Interlocked.Increment(ref _owner._pulls);
                return new ValueTask<bool>(_owner._never.Task);
            }

            public ValueTask DisposeAsync()
            {
                Interlocked.Increment(ref _owner._cancels);
                _owner._never.TrySetCanceled();
                return default;
            }
        }
    }

    private sealed class GatedSource : IAsyncEnumerable<string>
    {
        private readonly TaskCompletionSource<string> _chunk = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Enqueue(string value)
        {
            _chunk.TrySetResult(value);
        }

        public IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        private sealed class Enumerator : IAsyncEnumerator<string>
        {
            private readonly GatedSource _owner;
            private readonly CancellationTokenSource _cancellation;
            private int _stage;

            public Enumerator(GatedSource owner, CancellationToken cancellationToken)
            {
                _owner = owner;
                _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }

            public string Current { get; private set; } = string.Empty;

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_stage != 0)
                {
                    try
                    {
                        await Task.Delay(Timeout.Infinite, _cancellation.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }

                    return false;
                }

                _stage = 1;
                try
                {
                    Current = await _owner._chunk.Task.WaitAsync(_cancellation.Token).ConfigureAwait(false);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }

            public ValueTask DisposeAsync()
            {
                _cancellation.Cancel();
                return default;
            }
        }
    }
}
