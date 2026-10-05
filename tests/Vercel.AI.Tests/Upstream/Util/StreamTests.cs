// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class StreamTests
{
    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should read all chunks from a non-empty stream using async iteration", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Reads_every_chunk()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(new[] { "chunk1", "chunk2", "chunk3" }));
        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, await stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should handle an empty stream gracefully", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Reads_an_empty_stream()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(Array.Empty<string>()));
        Assert.Empty(await stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should maintain ReadableStream functionality", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Keeps_readable_stream_reads()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(new[] { "chunk1", "chunk2", "chunk3" }));
        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, await stream.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should cancel stream on early exit from for-await loop", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancels_on_early_return()
    {
        var cancelled = false;
        var source = OpenStream(new[] { "chunk1", "chunk2", "chunk3" }, delegate { cancelled = true; });
        var iterator = AsyncIterableStreams.CreateAsyncIterableStream(source).GetIterator();
        var collected = new List<string>();
        while (true)
        {
            var next = await iterator.NextAsync();
            if (next.Done)
            {
                break;
            }

            collected.Add(next.Value);
            if (next.Value == "chunk2")
            {
                await iterator.ReturnAsync();
                break;
            }
        }

        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should cancel stream when exception thrown inside for-await loop", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancels_when_the_iterator_throws()
    {
        var cancelled = false;
        var source = OpenStream(new[] { "chunk1", "chunk2", "chunk3" }, delegate { cancelled = true; });
        var iterator = AsyncIterableStreams.CreateAsyncIterableStream(source).GetIterator();
        var collected = new List<string>();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async delegate
        {
            while (true)
            {
                var next = await iterator.NextAsync();
                if (next.Done)
                {
                    break;
                }

                collected.Add(next.Value);
                if (next.Value == "chunk2")
                {
                    await iterator.ThrowAsync(new InvalidOperationException("Test error"));
                }
            }
        });
        Assert.Equal("Test error", error.Message);
        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
        Assert.True(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not cancel stream when exception thrown inside for-await loop", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Does_not_cancel_a_fully_consumed_stream()
    {
        var cancelled = false;
        var source = new ReadableStream<string>(
            delegate(ReadableStreamController<string> controller)
            {
                controller.Enqueue("chunk1");
                controller.Enqueue("chunk2");
                controller.Enqueue("chunk3");
                controller.Close();
            },
            cancel: delegate { cancelled = true; return Task.CompletedTask; });
        var values = await AsyncIterableStreams.CreateAsyncIterableStream(source).ToArrayAsync();
        Assert.Equal(new[] { "chunk1", "chunk2", "chunk3" }, values);
        Assert.False(cancelled);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not allow iterating twice after breaking", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Second_iteration_after_break_is_empty()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(new[] { "chunk1", "chunk2", "chunk3" }));
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

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should propagate errors from source stream to async iterable", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Propagates_a_source_error()
    {
        ReadableStreamController<string>? controller = null;
        var source = new ReadableStream<string>(delegate(ReadableStreamController<string> start)
        {
            controller = start;
            start.Enqueue("chunk1");
            start.Enqueue("chunk2");
        });
        var collected = new List<string>();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async delegate
        {
            await foreach (var chunk in AsyncIterableStreams.CreateAsyncIterableStream(source))
            {
                collected.Add(chunk);
                if (chunk == "chunk2")
                {
                    controller!.Error(new InvalidOperationException("Stream error"));
                }
            }
        });
        Assert.Equal("Stream error", error.Message);
        Assert.Equal(new[] { "chunk1", "chunk2" }, collected);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should stop async iterable when stream is cancelled", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task External_cancel_fails_the_held_iterator()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(new[] { "chunk1", "chunk2", "chunk3" }));
        var iterator = stream.GetIterator();
        var completed = false;
        Exception? caught = null;
        try
        {
            await iterator.NextAsync();
            await stream.CancelAsync();
            completed = true;
        }
        catch (Exception error)
        {
            caught = error;
        }
        finally
        {
            try
            {
                await iterator.ReturnAsync();
            }
            catch (Exception)
            {
            }
        }

        Assert.False(completed);
        Assert.NotNull(caught);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not collect any chunks when iterating on already cancelled stream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancelled_stream_yields_nothing()
    {
        var stream = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(new[] { "chunk1", "chunk2", "chunk3" }));
        await stream.CancelAsync();
        Assert.Empty(await stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::createAsyncIterableStream()::should not throw when return is called after the stream completed", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Return_after_completion_is_done()
    {
        var input = new[] { "chunk1", "chunk2", "chunk3" };
        var iterator = AsyncIterableStreams.CreateAsyncIterableStream(ReadableStream<string>.FromArray(input)).GetIterator();
        var output = new List<string>();
        while (true)
        {
            var next = await iterator.NextAsync();
            if (next.Done)
            {
                break;
            }

            output.Add(next.Value);
        }

        Assert.Equal(input, output);
        var returned = await iterator.ReturnAsync();
        Assert.True(returned.Done);
        Assert.False(returned.HasValue);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::%s read error cleanup::should release the reader and preserve the source error", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Releases_the_reader_and_preserves_the_error()
    {
        await AssertErrorCleanup(AsyncIterableStreams.CreateAsyncIterableStream);
        await AssertErrorCleanup(AsyncIterableStreams.AsAsyncIterableStream);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::%s read error cleanup::should release the reader when the source error is undefined", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Releases_the_reader_when_the_error_is_undefined()
    {
        await AssertUndefinedError(AsyncIterableStreams.CreateAsyncIterableStream);
        await AssertUndefinedError(AsyncIterableStreams.AsAsyncIterableStream);
    }

    [UpstreamTest("packages/ai/src/util/async-iterable-stream.test.ts::%s read error cleanup::should preserve the source error for concurrent reads and release the reader once", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Concurrent_reads_share_one_error()
    {
        await AssertConcurrentError(AsyncIterableStreams.CreateAsyncIterableStream);
        await AssertConcurrentError(AsyncIterableStreams.AsAsyncIterableStream);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return no stream when immediately closed", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Closed_stitch_is_empty()
    {
        var stitch = new StitchableStream<int>();
        stitch.Close();
        Assert.Empty(await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from a single inner stream", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Stitches_one_stream()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2, 3 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3 }, await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from 2 inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Stitches_two_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2, 3 }));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 4, 5, 6 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should return all values from 3 inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Stitches_three_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2, 3 }));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 4, 5, 6 }));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 7, 8, 9 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should handle empty inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Skips_empty_inner_streams()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(ReadableStream<int>.FromArray(Array.Empty<int>()));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2 }));
        stitch.AddStream(ReadableStream<int>.FromArray(Array.Empty<int>()));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 3, 4 }));
        stitch.Close();
        Assert.Equal(new[] { 1, 2, 3, 4 }, await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read full streams after they are added::should handle reading a single value before it is added", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Reads_a_value_added_after_the_read_starts()
    {
        var stitch = new StitchableStream<int>();
        var reader = stitch.Stream.GetReader();
        var pending = reader.ReadAsync();
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 42 }));
        stitch.Close();
        var first = await pending;
        Assert.False(first.Done);
        Assert.Equal(42, first.Value);
        var done = await reader.ReadAsync();
        Assert.True(done.Done);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > read from partial stream and with interruptions::should return all values from 2 inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Delivers_values_to_reads_started_early()
    {
        var stitch = new StitchableStream<int>();
        var reader = stitch.Stream.GetReader();
        var reads = new Task<StreamRead<int>>[5];
        for (var i = 0; i < reads.Length; i++)
        {
            reads[i] = reader.ReadAsync();
        }

        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2, 3 }));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 4, 5 }));
        stitch.Close();
        var done = await reader.ReadAsync();
        Assert.True(done.Done);
        for (var i = 0; i < reads.Length; i++)
        {
            var item = await reads[i];
            Assert.False(item.Done);
            Assert.Equal(i + 1, item.Value);
        }
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > error handling::should handle errors from inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Surfaces_an_inner_stream_error()
    {
        var stitch = new StitchableStream<int>();
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2 }));
        stitch.AddStream(new ReadableStream<int>(delegate(ReadableStreamController<int> controller)
        {
            controller!.Error(new InvalidOperationException("Test error"));
        }));
        stitch.AddStream(ReadableStream<int>.FromArray(new[] { 3, 4 }));
        stitch.Close();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(delegate { return stitch.Stream.ToArrayAsync(); });
        Assert.Equal("Test error", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > error handling::should call the inner stream error callback", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Calls_the_inner_error_callback()
    {
        var stitch = new StitchableStream<int>();
        var failure = new InvalidOperationException("Test error");
        object? received = null;
        stitch.AddStream(
            new ReadableStream<int>(delegate(ReadableStreamController<int> controller) { controller!.Error(failure); }),
            delegate(object error) { received = error; });
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(delegate { return stitch.Stream.ToArrayAsync(); });
        Assert.Same(failure, thrown);
        Assert.Same(failure, received);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should cancel all inner streams when cancelled", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Cancels_inner_streams()
    {
        var stitch = new StitchableStream<int>();
        var first = false;
        var second = false;
        stitch.AddStream(OpenStream(new[] { 1, 2 }, delegate { first = true; }));
        stitch.AddStream(OpenStream(new[] { 3, 4 }, delegate { second = true; }));
        await stitch.Stream.CancelAsync();
        Assert.True(first);
        Assert.True(second);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should call the inner stream cancellation callbacks", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Calls_inner_cancel_callbacks()
    {
        var stitch = new StitchableStream<int>();
        var first = false;
        var second = false;
        stitch.AddStream(new ReadableStream<int>(), null, delegate { first = true; });
        stitch.AddStream(new ReadableStream<int>(), null, delegate { second = true; });
        await stitch.Stream.CancelAsync();
        Assert.True(first);
        Assert.True(second);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should discard streams added after cancellation and allow closing", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Discards_a_stream_added_after_cancel()
    {
        var stitch = new StitchableStream<int>();
        var cancelled = false;
        await stitch.Stream.CancelAsync();
        stitch.AddStream(new ReadableStream<int>(cancel: delegate { cancelled = true; return Task.CompletedTask; }));
        await Task.Yield();
        Assert.True(cancelled);
        stitch.Close();
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > cancellation & closing::should throw an error when adding a stream after closing", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_a_stream_added_after_close()
    {
        var stitch = new StitchableStream<int>();
        stitch.Close();
        var error = Assert.Throws<InvalidOperationException>(delegate
        {
            stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1, 2 }));
        });
        Assert.Equal("Cannot add inner stream: outer stream is closed", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > terminate::should immediately close the stream and cancel all inner streams", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Terminate_cancels_inners_and_closes()
    {
        var stitch = new StitchableStream<int>();
        var first = false;
        var second = false;
        stitch.AddStream(OpenStream(new[] { 1, 2 }, delegate { first = true; }));
        stitch.AddStream(OpenStream(new[] { 3, 4 }, delegate { second = true; }));
        stitch.Terminate();
        Assert.True(first);
        Assert.True(second);
        Assert.Empty(await stitch.Stream.ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/create-stitchable-stream.test.ts::createStitchableStream > terminate::should throw an error when adding a stream after terminating", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_a_stream_added_after_terminate()
    {
        var stitch = new StitchableStream<int>();
        stitch.Terminate();
        var error = Assert.Throws<InvalidOperationException>(delegate
        {
            stitch.AddStream(ReadableStream<int>.FromArray(new[] { 1 }));
        });
        Assert.Equal("Cannot add inner stream: outer stream is closed", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should create a readable stream with provided values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Simulates_the_provided_values()
    {
        var values = new[] { "a", "b", "c" };
        Assert.Equal(values, await SimulateReadableStream.Create(values).ToArrayAsync());
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should respect the chunkDelayInMs setting", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Records_initial_and_chunk_delays()
    {
        var delays = new List<int?>();
        await SimulateReadableStream.Create(new[] { 1, 2, 3 }, 500, 100, delegate(int? milliseconds)
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }).ToArrayAsync();
        Assert.Equal(new int?[] { 500, 100, 100 }, delays);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should handle empty values array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Empty_simulation_is_done()
    {
        var read = await SimulateReadableStream.Create(Array.Empty<string>()).GetReader().ReadAsync();
        Assert.True(read.Done);
        Assert.False(read.HasValue);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should handle different types of values", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Simulates_object_chunks()
    {
        var chunks = new[] { new SimChunk(1, "hello"), new SimChunk(2, "world") };
        var values = await SimulateReadableStream.Create(chunks).ToArrayAsync();
        Assert.Equal(1, values[0].Id);
        Assert.Equal("hello", values[0].Text);
        Assert.Equal(2, values[1].Id);
        Assert.Equal("world", values[1].Text);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should skip all delays when both delay settings are null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Passes_null_when_both_delays_are_null()
    {
        var delays = new List<int?>();
        await SimulateReadableStream.Create(new[] { 1, 2, 3 }, null, null, delegate(int? milliseconds)
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }).ToArrayAsync();
        Assert.Equal(new int?[] { null, null, null }, delays);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should apply chunk delays but skip initial delay when initialDelayInMs is null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Skips_only_the_initial_delay()
    {
        var delays = new List<int?>();
        await SimulateReadableStream.Create(new[] { 1, 2, 3 }, null, 100, delegate(int? milliseconds)
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }).ToArrayAsync();
        Assert.Equal(new int?[] { null, 100, 100 }, delays);
    }

    [UpstreamTest("packages/ai/src/util/simulate-readable-stream.test.ts::simulateReadableStream::should apply initial delay but skip chunk delays when chunkDelayInMs is null", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Skips_only_the_chunk_delays()
    {
        var delays = new List<int?>();
        await SimulateReadableStream.Create(new[] { 1, 2, 3 }, 500, null, delegate(int? milliseconds)
        {
            delays.Add(milliseconds);
            return Task.CompletedTask;
        }).ToArrayAsync();
        Assert.Equal(new int?[] { 500, null, null }, delays);
    }

    private static async Task AssertErrorCleanup(Func<ReadableStream<string>, AsyncIterableStream<string>> create)
    {
        var sourceError = new InvalidOperationException("source failed");
        var cancelCalls = 0;
        ReadableStreamController<string>? controller = null;
        var stream = create(new ReadableStream<string>(
            delegate(ReadableStreamController<string> start) { controller = start; },
            cancel: delegate { cancelCalls++; return Task.CompletedTask; }));
        var iterator = stream.GetIterator();
        var failed = iterator.NextAsync();
        controller!.Error(sourceError);
        Assert.Same(sourceError, await Assert.ThrowsAsync<InvalidOperationException>(delegate { return failed; }));
        Assert.False(stream.Locked);
        Assert.Equal(0, cancelCalls);
        var next = await iterator.NextAsync();
        Assert.True(next.Done);
        var reader = stream.GetReader();
        var again = await reader.ReadAsync();
        Assert.True(again.Rejected);
        Assert.Same(sourceError, again.Reason);
        reader.ReleaseLock();
        var returned = await iterator.ReturnAsync();
        Assert.True(returned.Done);
    }

    private static async Task AssertUndefinedError(Func<ReadableStream<string>, AsyncIterableStream<string>> create)
    {
        ReadableStreamController<string>? controller = null;
        var stream = create(new ReadableStream<string>(delegate(ReadableStreamController<string> start) { controller = start; }));
        var iterator = stream.GetIterator();
        var failed = iterator.NextAsync();
        controller!.Error(null);
        var error = await Assert.ThrowsAsync<UndefinedStreamError>(delegate { return failed; });
        Assert.Null(error.Reason);
        Assert.False(stream.Locked);
        Assert.True((await iterator.NextAsync()).Done);
    }

    private static async Task AssertConcurrentError(Func<ReadableStream<string>, AsyncIterableStream<string>> create)
    {
        var sourceError = new InvalidOperationException("source failed");
        var cancelCalls = 0;
        ReadableStreamController<string>? controller = null;
        var stream = create(new ReadableStream<string>(
            delegate(ReadableStreamController<string> start) { controller = start; },
            cancel: delegate { cancelCalls++; return Task.CompletedTask; }));
        var iterator = stream.GetIterator();
        var first = iterator.NextAsync();
        var second = iterator.NextAsync();
        controller!.Error(sourceError);
        Assert.Same(sourceError, await Assert.ThrowsAsync<InvalidOperationException>(delegate { return first; }));
        Assert.Same(sourceError, await Assert.ThrowsAsync<InvalidOperationException>(delegate { return second; }));
        Assert.False(stream.Locked);
        Assert.Equal(0, cancelCalls);
        Assert.True((await iterator.NextAsync()).Done);
    }

    private static ReadableStream<T> OpenStream<T>(IReadOnlyList<T> values, Action onCancel)
    {
        return new ReadableStream<T>(
            delegate(ReadableStreamController<T> controller)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    controller.Enqueue(values[i]);
                }
            },
            cancel: delegate
            {
                onCancel();
                return Task.CompletedTask;
            });
    }

    private sealed class SimChunk
    {
        public SimChunk(int id, string text)
        {
            Id = id;
            Text = text;
        }

        public int Id { get; }

        public string Text { get; }
    }
}
