// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.CreateSseStreamWithKeepAlive;

/// <summary>
/// Port of <c>create-sse-stream-with-keep-alive.test.ts</c> &gt; <c>createSseStreamWithKeepAlive</c>.
/// Upstream drives the timers with fake timers. These tests use short real delays with wide margins.
/// </summary>
public sealed class CreateSseStreamWithKeepAliveTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/create-sse-stream-with-keep-alive.test.ts::createSseStreamWithKeepAlive::";
    private const string StreamOpen = ": stream-open\n\n";
    private const string KeepAlive = ": keep-alive\n\n";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through the original stream when keepAliveMs is undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_pass_through_the_original_stream_when_keepAliveMs_is_undefined()
    {
        var stream = ReadableStream<string>.FromArray(["data"]);

        Assert.Same(stream, UIMessageStreamResult.CreateSseStreamWithKeepAlive(stream, null));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should send an opening comment immediately and comments while the source is idle",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_an_opening_comment_immediately_and_comments_while_the_source_is_idle()
    {
        var stream = new ReadableStream<string>();
        var reader = UIMessageStreamResult.CreateSseStreamWithKeepAlive(stream, 100).GetReader();

        Assert.Equal(StreamOpen, await Next(reader));
        Assert.Equal(KeepAlive, await Next(reader));

        await reader.CancelAsync();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should retain one pending source read across many idle keep-alives",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_one_pending_source_read_across_many_idle_keep_alives()
    {
        var source = new PendingSource();
        var reader = ReadableStream<string>
            .From(UIMessageStreamResult.CreateSseStreamWithKeepAlive(source, 10))
            .GetReader();

        Assert.Equal(StreamOpen, await Next(reader));
        for (var i = 0; i < 25; i++)
        {
            Assert.Equal(KeepAlive, await Next(reader));
        }

        Assert.Equal(1, source.MoveNextCalls);

        await reader.CancelAsync();
        Assert.Equal(1, source.DisposeCalls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reset the keep-alive timer after source activity",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reset_the_keep_alive_timer_after_source_activity()
    {
        ReadableStreamController<string> sourceController = null!;
        var stream = new ReadableStream<string>(start: controller =>
            sourceController = controller
        );
        var reader = UIMessageStreamResult.CreateSseStreamWithKeepAlive(stream, 600).GetReader();

        await Next(reader);
        var sourceRead = reader.ReadAsync();
        await Task.Delay(300);
        sourceController.Enqueue("data");

        Assert.Equal("data", (await sourceRead.WaitAsync(Limit)).Value);

        var keepAlive = reader.ReadAsync();
        await Task.Delay(300);
        Assert.False(keepAlive.IsCompleted);

        Assert.Equal(KeepAlive, (await keepAlive.WaitAsync(Limit)).Value);

        await reader.CancelAsync();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve source chunks and completion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_source_chunks_and_completion()
    {
        var stream = UIMessageStreamResult.CreateSseStreamWithKeepAlive(
            ReadableStream<string>.FromArray(["data 1", "data 2"]),
            100
        );

        Assert.Equal([StreamOpen, "data 1", "data 2"], await stream.ToArrayAsync());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should cancel the source stream",
        Coverage = UpstreamCoverage.Partial,
        Note = "ReadableStream.CancelAsync takes no reason, so the cancel reason is not forwarded or asserted."
    )]
    public async Task Should_cancel_the_source_stream()
    {
        var cancelCalls = 0;
        var stream = new ReadableStream<string>(cancel: () =>
        {
            Interlocked.Increment(ref cancelCalls);
            return Task.CompletedTask;
        });
        var reader = UIMessageStreamResult.CreateSseStreamWithKeepAlive(stream, 100).GetReader();

        await Next(reader);
        await reader.CancelAsync();

        Assert.Equal(1, Volatile.Read(ref cancelCalls));
    }

    private static async Task<string> Next(ReadableStreamReader<string> reader)
    {
        var read = await reader.ReadAsync().WaitAsync(Limit);
        Assert.False(read.Done);
        return read.Value;
    }

    // A source whose single read never completes, unless it is cancelled.
    private sealed class PendingSource : IAsyncEnumerable<string>
    {
        private int _moveNextCalls;
        private int _disposeCalls;

        public int MoveNextCalls => Volatile.Read(ref _moveNextCalls);

        public int DisposeCalls => Volatile.Read(ref _disposeCalls);

        public IAsyncEnumerator<string> GetAsyncEnumerator(
            CancellationToken cancellationToken = default
        ) => new Enumerator(this, cancellationToken);

        private sealed class Enumerator(PendingSource owner, CancellationToken token)
            : IAsyncEnumerator<string>
        {
            public string Current => string.Empty;

            public async ValueTask<bool> MoveNextAsync()
            {
                Interlocked.Increment(ref owner._moveNextCalls);
                await Task.Delay(Timeout.Infinite, token).ConfigureAwait(false);
                return false;
            }

            public ValueTask DisposeAsync()
            {
                Interlocked.Increment(ref owner._disposeCalls);
                return default;
            }
        }
    }
}
