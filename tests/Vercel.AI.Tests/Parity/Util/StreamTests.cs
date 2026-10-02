// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class StreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should read all chunks from a non-empty stream using async iteration",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_every_chunk()
    {
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(new[] { "chunk1", "chunk2", "chunk3" }));
        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, await ReadAll(stream));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should handle an empty stream gracefully", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_an_empty_stream()
    {
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(Array.Empty<string>()));
        Assert.Empty(await ReadAll(stream));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should maintain ReadableStream functionality", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_through_a_reader()
    {
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(new[] { "chunk1", "chunk2", "chunk3" }));
        var reader = stream.GetReader();
        var values = new List<string>();
        while (true)
        {
            var next = await reader.NextAsync();
            if (next.Done)
            {
                break;
            }

            values.Add(next.Value);
        }

        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, values);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should cancel stream on early exit from for-await loop",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_source_on_an_early_return()
    {
        var cancelled = false;
        var source = new AsyncIterableStream<string> { OnCancel = () => cancelled = true };
        source.Enqueue("chunk1");
        source.Enqueue("chunk2");
        source.Enqueue("chunk3");
        var stream = AsyncIterableStreams.Create(source);
        var reader = stream.GetReader();
        var collected = new List<string>();
        while (true)
        {
            var next = await reader.NextAsync();
            if (next.Done)
            {
                break;
            }

            collected.Add(next.Value);
            if (next.Value == "chunk2")
            {
                await reader.ReturnAsync();
                break;
            }
        }

        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should cancel stream when exception thrown inside for-await loop",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_source_when_the_reader_throws()
    {
        var cancelled = false;
        var source = new AsyncIterableStream<string> { OnCancel = () => cancelled = true };
        source.Enqueue("chunk1");
        source.Enqueue("chunk2");
        source.Enqueue("chunk3");
        var stream = AsyncIterableStreams.Create(source);
        var reader = stream.GetReader();
        var collected = new List<string>();
        var error = new InvalidOperationException("Test error");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            while (true)
            {
                var next = await reader.NextAsync();
                if (next.Done)
                {
                    break;
                }

                collected.Add(next.Value);
                if (next.Value == "chunk2")
                {
                    await reader.ThrowAsync(error);
                }
            }
        });
        Assert.Same(error, thrown);
        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
        Assert.True(cancelled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not cancel stream when exception thrown inside for-await loop",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_cancel_a_fully_consumed_stream()
    {
        var cancelled = false;
        var source = new AsyncIterableStream<string> { OnCancel = () => cancelled = true };
        source.Enqueue("chunk1");
        source.Enqueue("chunk2");
        source.Enqueue("chunk3");
        source.Complete();
        var stream = AsyncIterableStreams.Create(source);
        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, await ReadAll(stream));
        Assert.False(cancelled);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not allow iterating twice after breaking", Coverage = UpstreamCoverage.Covered)]
    public async Task A_second_iteration_after_break_yields_nothing()
    {
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(new[] { "chunk1", "chunk2", "chunk3" }));
        var collected = new List<string>();
        await foreach (var chunk in stream)
        {
            collected.Add(chunk);
            if (chunk == "chunk1")
            {
                break;
            }
        }

        Assert.Equal(new[] { "chunk1" }, collected);
        await foreach (var chunk in stream)
        {
            collected.Add(chunk);
        }

        Assert.Equal(new[] { "chunk1" }, collected);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should propagate errors from source stream to async iterable",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Propagates_a_source_error()
    {
        var source = new AsyncIterableStream<string>();
        source.Enqueue("chunk1");
        source.Enqueue("chunk2");
        var stream = AsyncIterableStreams.Create(source);
        var collected = new List<string>();
        var error = new InvalidOperationException("Stream error");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var chunk in stream)
            {
                collected.Add(chunk);
                if (chunk == "chunk2")
                {
                    source.Fail(error);
                }
            }
        });
        Assert.Same(error, thrown);
        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should stop async iterable when stream is cancelled", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancel_while_locked_throws()
    {
        var source = AsyncIterableStream<string>.From(new[] { "chunk1", "chunk2", "chunk3" });
        var stream = AsyncIterableStreams.Create(source);
        var reader = stream.GetReader();
        var iterationCompleted = false;
        Exception? errorCaught = null;
        try
        {
            await reader.NextAsync();
            stream.Cancel();
            iterationCompleted = true;
        }
        catch (Exception error)
        {
            errorCaught = error;
        }

        Assert.False(iterationCompleted);
        Assert.NotNull(errorCaught);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not collect any chunks when iterating on already cancelled stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task A_cancelled_stream_yields_nothing()
    {
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(new[] { "chunk1", "chunk2", "chunk3" }));
        stream.Cancel();
        Assert.Empty(await ReadAll(stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not throw when return is called after the stream completed",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Return_after_completion_does_not_throw()
    {
        var input = new[] { "chunk1", "chunk2", "chunk3" };
        var stream = AsyncIterableStreams.Create(AsyncIterableStream<string>.From(input));
        var reader = stream.GetReader();
        var output = new List<string>();
        while (true)
        {
            var next = await reader.NextAsync();
            if (next.Done)
            {
                break;
            }

            output.Add(next.Value);
        }

        Assert.Equal(input, output);
        var returned = await reader.ReturnAsync();
        Assert.True(returned.Done);
        Assert.False(returned.HasValue);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::%s read error cleanup::should release the reader and preserve the source error",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Releases_the_reader_and_preserves_the_source_error()
    {
        await AssertErrorCleanup(source => AsyncIterableStreams.Create(source));
        await AssertErrorCleanup(source => AsyncIterableStreams.As(source));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/async-iterable-stream.test.ts::%s read error cleanup::should preserve the source error for concurrent reads and release the reader once",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_the_source_error_for_concurrent_reads()
    {
        await AssertConcurrentErrorCleanup(source => AsyncIterableStreams.Create(source));
        await AssertConcurrentErrorCleanup(source => AsyncIterableStreams.As(source));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should create a readable stream with provided values",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Simulates_the_provided_values()
    {
        var values = new[] { "a", "b", "c" };
        Assert.Equal(values, await ReadAll(ReadableStreams.Simulate(values)));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should respect the chunkDelayInMs setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Records_initial_and_chunk_delays()
    {
        var delays = new List<int?>();
        await ReadAll(ReadableStreams.Simulate(new[] { 1, 2, 3 }, 500, 100, milliseconds =>
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }));
        Assert.Equal(new int?[] { 500, 100, 100 }, delays);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should handle empty values array", Coverage = UpstreamCoverage.Covered)]
    public async Task An_empty_simulation_is_done()
    {
        var reader = ReadableStreams.Simulate(Array.Empty<int>()).GetReader();
        var next = await reader.NextAsync();
        Assert.True(next.Done);
        Assert.False(next.HasValue);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should handle different types of values", Coverage = UpstreamCoverage.Covered)]
    public async Task Simulates_object_values()
    {
        var values = new[] { new Item(1, "hello"), new Item(2, "world") };
        var read = await ReadAll(ReadableStreams.Simulate(values));
        Assert.Equal(2, read.Count);
        Assert.Equal(1, read[0].Id);
        Assert.Equal("hello", read[0].Text);
        Assert.Equal(2, read[1].Id);
        Assert.Equal("world", read[1].Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should skip all delays when both delay settings are null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Invokes_the_delay_with_null_for_both_settings()
    {
        var delays = new List<int?>();
        await ReadAll(ReadableStreams.Simulate(new[] { 1, 2, 3 }, null, null, milliseconds =>
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }));
        Assert.Equal(new int?[] { null, null, null }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should apply chunk delays but skip initial delay when initialDelayInMs is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_only_the_initial_delay()
    {
        var delays = new List<int?>();
        await ReadAll(ReadableStreams.Simulate(new[] { 1, 2, 3 }, null, 100, milliseconds =>
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }));
        Assert.Equal(new int?[] { null, 100, 100 }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should apply initial delay but skip chunk delays when chunkDelayInMs is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_only_the_chunk_delays()
    {
        var delays = new List<int?>();
        await ReadAll(ReadableStreams.Simulate(new[] { 1, 2, 3 }, 500, null, milliseconds =>
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }));
        Assert.Equal(new int?[] { 500, null, null }, delays);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return no stream when immediately closed",
        Coverage = UpstreamCoverage.Covered)]
    public async Task An_immediately_closed_stitch_is_empty()
    {
        var stitch = new StitchableStream<int>();
        stitch.Close();
        Assert.Empty(await ReadAll(stitch.Stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from a single inner stream",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stitches_one_inner_stream()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2, 3 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3 }, await ReadAll(stitch.Stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from 2 inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stitches_two_inner_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2, 3 }));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 4, 5, 6 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, await ReadAll(stitch.Stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from 3 inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stitches_three_inner_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2, 3 }));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 4, 5, 6 }));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 7, 8, 9 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, await ReadAll(stitch.Stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should handle empty inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Skips_empty_inner_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(AsyncIterableStream<int>.From(Array.Empty<int>()));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2 }));
        stitch.AddStream(AsyncIterableStream<int>.From(Array.Empty<int>()));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 3, 4 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4 }, await ReadAll(stitch.Stream));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should handle reading a single value before it is added",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_a_value_added_after_the_read_starts()
    {
        var stitch = new StitchableStream<int>();
        var reader = stitch.Stream.GetReader();
        var pending = reader.NextAsync();
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 42 }));
        stitch.Close();
        var first = await pending;
        Assert.False(first.Done);
        Assert.Equal(42, first.Value);
        var done = await reader.NextAsync();
        Assert.True(done.Done);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read from partial stream and with interruptions::should return all values from 2 inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Satisfies_reads_that_started_before_the_inners_were_added()
    {
        var stitch = new StitchableStream<int>();
        var reader = stitch.Stream.GetReader();
        var pending = new List<Task<StreamRead<int>>>();
        for (var i = 0; i < 5; i++)
        {
            pending.Add(reader.NextAsync());
        }

        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2, 3 }));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 4, 5 }));
        stitch.Close();
        var done = await reader.NextAsync();
        Assert.True(done.Done);
        for (var i = 0; i < 5; i++)
        {
            var item = await pending[i];
            Assert.False(item.Done);
            Assert.Equal(i + 1, item.Value);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > error handling::should handle errors from inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_an_inner_stream_error()
    {
        var stitch = new StitchableStream<int>();
        var failed = new AsyncIterableStream<int>();
        failed.Fail(new InvalidOperationException("Test error"));
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2 }));
        stitch.AddStream(failed);
        stitch.AddStream(AsyncIterableStream<int>.From(new[] { 3, 4 }));
        stitch.Close();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAll(stitch.Stream));
        Assert.Equal("Test error", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > error handling::should call the inner stream error callback",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_the_inner_error_callback_with_the_same_exception()
    {
        var stitch = new StitchableStream<int>();
        var error = new InvalidOperationException("Test error");
        Exception? received = null;
        var failed = new AsyncIterableStream<int>();
        failed.Fail(error);
        stitch.AddStream(failed, receivedError => received = receivedError);
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAll(stitch.Stream));
        Assert.Same(error, thrown);
        Assert.Same(error, received);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should cancel all inner streams when cancelled",
        Coverage = UpstreamCoverage.Covered)]
    public void Cancels_every_inner_stream()
    {
        var stitch = new StitchableStream<int>();
        var firstCancelled = false;
        var secondCancelled = false;
        var first = new AsyncIterableStream<int> { OnCancel = () => firstCancelled = true };
        first.Enqueue(1);
        first.Enqueue(2);
        var second = new AsyncIterableStream<int> { OnCancel = () => secondCancelled = true };
        second.Enqueue(3);
        second.Enqueue(4);
        stitch.AddStream(first);
        stitch.AddStream(second);
        stitch.Stream.Cancel();
        Assert.True(firstCancelled);
        Assert.True(secondCancelled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should call the inner stream cancellation callbacks",
        Coverage = UpstreamCoverage.Covered)]
    public void Calls_inner_cancellation_callbacks()
    {
        var stitch = new StitchableStream<int>();
        var first = false;
        var second = false;
        stitch.AddStream(new AsyncIterableStream<int>(), onCancel: () => first = true);
        stitch.AddStream(new AsyncIterableStream<int>(), onCancel: () => second = true);
        stitch.Stream.Cancel();
        Assert.True(first);
        Assert.True(second);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should discard streams added after cancellation and allow closing",
        Coverage = UpstreamCoverage.Covered)]
    public void Discards_a_stream_added_after_cancellation()
    {
        var stitch = new StitchableStream<int>();
        var lateCancelled = false;
        stitch.Stream.Cancel();
        var late = new AsyncIterableStream<int> { OnCancel = () => lateCancelled = true };
        stitch.AddStream(late);
        Assert.True(lateCancelled);
        stitch.Close();
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should throw an error when adding a stream after closing",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_stream_is_added_after_close()
    {
        var stitch = new StitchableStream<int>();
        stitch.Close();
        var error = Assert.Throws<InvalidOperationException>(() => stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2 })));
        Assert.Equal("Cannot add inner stream: outer stream is closed", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > terminate::should immediately close the stream and cancel all inner streams",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Terminate_closes_and_cancels_inners()
    {
        var stitch = new StitchableStream<int>();
        var firstCancelled = false;
        var secondCancelled = false;
        var first = new AsyncIterableStream<int> { OnCancel = () => firstCancelled = true };
        first.Enqueue(1);
        first.Enqueue(2);
        var second = new AsyncIterableStream<int> { OnCancel = () => secondCancelled = true };
        second.Enqueue(3);
        second.Enqueue(4);
        stitch.AddStream(first);
        stitch.AddStream(second);
        var reader = stitch.Stream.GetReader();
        var firstRead = await reader.NextAsync();
        stitch.Terminate();
        var finalRead = await reader.NextAsync();
        Assert.False(firstRead.Done);
        Assert.Equal(1, firstRead.Value);
        Assert.True(finalRead.Done);
        Assert.True(firstCancelled);
        Assert.True(secondCancelled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > terminate::should throw an error when adding a stream after terminating",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_a_stream_is_added_after_terminate()
    {
        var stitch = new StitchableStream<int>();
        stitch.Terminate();
        var error = Assert.Throws<InvalidOperationException>(() => stitch.AddStream(AsyncIterableStream<int>.From(new[] { 1, 2 })));
        Assert.Equal("Cannot add inner stream: outer stream is closed", error.Message);
    }

    private static async Task AssertErrorCleanup(Func<AsyncIterableStream<string>, AsyncIterableStream<string>> create)
    {
        var sourceError = new InvalidOperationException("source failed");
        var cancelCalls = 0;
        var source = new AsyncIterableStream<string> { OnCancel = () => cancelCalls++ };
        var stream = create(source);
        var reader = stream.GetReader();
        var failed = reader.NextAsync();
        source.Fail(sourceError);
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => failed);
        Assert.Same(sourceError, thrown);
        Assert.False(stream.IsLocked);
        Assert.Equal(0, cancelCalls);
        var done = await reader.NextAsync();
        Assert.True(done.Done);
        var another = stream.GetReader();
        var again = await Assert.ThrowsAsync<InvalidOperationException>(() => another.NextAsync());
        Assert.Same(sourceError, again);
        another.Release();
        var returned = await reader.ReturnAsync();
        Assert.True(returned.Done);
    }

    private static async Task AssertConcurrentErrorCleanup(Func<AsyncIterableStream<string>, AsyncIterableStream<string>> create)
    {
        var sourceError = new InvalidOperationException("source failed");
        var cancelCalls = 0;
        var source = new AsyncIterableStream<string> { OnCancel = () => cancelCalls++ };
        var stream = create(source);
        var reader = stream.GetReader();
        var first = reader.NextAsync();
        var second = reader.NextAsync();
        source.Fail(sourceError);
        var firstError = await Assert.ThrowsAsync<InvalidOperationException>(() => first);
        var secondError = await Assert.ThrowsAsync<InvalidOperationException>(() => second);
        Assert.Same(sourceError, firstError);
        Assert.Same(sourceError, secondError);
        Assert.False(stream.IsLocked);
        Assert.Equal(0, cancelCalls);
        var done = await reader.NextAsync();
        Assert.True(done.Done);
    }

    private static async Task<List<T>> ReadAll<T>(AsyncIterableStream<T> stream)
    {
        var values = new List<T>();
        await foreach (var value in stream)
        {
            values.Add(value);
        }

        return values;
    }

    private sealed class Item
    {
        public Item(int id, string text)
        {
            Id = id;
            Text = text;
        }

        public int Id { get; }

        public string Text { get; }
    }
}
