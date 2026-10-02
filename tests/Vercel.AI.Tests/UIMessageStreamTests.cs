// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

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
    public async Task Writes_opening_and_keep_alive_comments_while_the_source_is_idle()
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
    public async Task ToUIMessageStreamResult_writes_stream_open_before_events()
    {
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
    public async Task Keep_alive_stops_when_cancellation_is_requested()
    {
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
}
