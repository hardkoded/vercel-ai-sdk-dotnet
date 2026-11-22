// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.Telemetry;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamTextWithMinimalRetention;

/// <summary>Port of <c>stream-text-with-minimal-retention.test.ts</c> &gt; <c>streaming-only contract</c>. Only the tests below are ported.</summary>
[Collection("TelemetryRegistry")]
public sealed class StreamingOnlyContractTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-with-minimal-retention.test.ts::streaming-only contract::";

    internal static StreamTextSingleConsumerResult LowMemory(
        IReadOnlyList<LanguageModelStreamPart> parts,
        OutputSpec? output = null
    )
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextLowMemoryAsync(
            new StreamTextOptions
            {
                Model = new TestLanguageModel { StreamParts = parts },
                Prompt = "test",
                Output = output,
            }
        );
    }

    internal static StreamTextSingleConsumerResult LowMemory(StreamTextOptions options)
    {
        return new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextLowMemoryAsync(options);
    }

    private static DelegateModel ModelOf(Func<IAsyncEnumerable<LanguageModelStreamPart>> stream)
    {
        return new DelegateModel
        {
            Stream = (_, _) => Task.FromResult(new LanguageModelStreamResult(stream())),
        };
    }

    internal static LanguageModelStreamPart[] TextParts(string text = "hello")
    {
        return
        [
            new TextStartStreamPart("text"),
            new TextDeltaStreamPart("text", text),
            new TextEndStreamPart("text"),
            Finish(),
        ];
    }

    internal static FinishStreamPart Finish()
    {
        return new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 2, 5), "stop");
    }

    private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> values)
    {
        var list = new List<T>();
        await foreach (var value in values)
        {
            list.Add(value);
        }

        return list;
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a duplicate consumeStream without corrupting metadata")]
    public async Task Rejects_a_duplicate_consumeStream_without_corrupting_metadata()
    {
        var result = LowMemory(TextParts());
        var stream = result.TextStream();

        var error = await Assert.ThrowsAsync<AiSdkException>(() => result.ConsumeStreamAsync());
        Assert.Contains("one stream consumer", error.Message);
        Assert.Equal(new[] { "hello" }, await ToList(stream));
        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "cancels the upstream stream and rejects metadata when iteration stops early",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Cancels_the_upstream_stream_and_rejects_metadata_when_iteration_stops_early()
    {
        var cancelled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var result = LowMemory(
            new StreamTextOptions { Model = ModelOf(() => Endless(cancelled)), Prompt = "test" }
        );

        var assertion = Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.Usage);
        await foreach (var delta in result.TextStream())
        {
            Assert.Equal("hello", delta);
            break;
        }

        await assertion;
        await cancelled.Task.WaitAsync(SettleTimeout);

        static async IAsyncEnumerable<LanguageModelStreamPart> Endless(
            TaskCompletionSource cancelled
        )
        {
            try
            {
                yield return new TextStartStreamPart("text");
                while (true)
                {
                    yield return new TextDeltaStreamPart("text", "hello");
                    await Task.Yield();
                }
            }
            finally
            {
                cancelled.TrySetResult();
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "honors abort signals and preserves onAbort lifecycle delivery",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Honors_abort_signals_and_preserves_onAbort_lifecycle_delivery()
    {
        var abort = new AbortController();
        var onAbort = new List<StreamAbortContext>();
        var cancelled = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var result = LowMemory(
            new StreamTextOptions
            {
                Model = ModelOf(() => OpenStream(cancelled)),
                Prompt = "test",
                AbortSignal = abort.Signal,
                OnAbort = (context, _) =>
                {
                    onAbort.Add(context);
                    return Task.CompletedTask;
                },
            }
        );

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
            if (part is TextDeltaPart)
            {
                abort.Abort("stop");
            }
        }

        Assert.Contains(parts, part => part is AbortPart { Reason: "stop" });
        var context = Assert.Single(onAbort);
        Assert.Contains("stop", context.Exception.Message);
        await cancelled.Task.WaitAsync(SettleTimeout);

        static async IAsyncEnumerable<LanguageModelStreamPart> OpenStream(
            TaskCompletionSource cancelled,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            try
            {
                yield return new TextStartStreamPart("text");
                yield return new TextDeltaStreamPart("text", "hello");
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            finally
            {
                cancelled.TrySetResult();
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "applies backpressure to a slow consumer",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Applies_backpressure_to_a_slow_consumer()
    {
        var emitted = 0;
        var result = LowMemory(new StreamTextOptions { Model = ModelOf(Pulled), Prompt = "test" });

        await using var enumerator = result.TextStream().GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        await Task.Delay(10);

        // The existing orchestration has several bounded stream stages.
        Assert.True(Volatile.Read(ref emitted) < 30, $"emitted {emitted} parts");

        async IAsyncEnumerable<LanguageModelStreamPart> Pulled()
        {
            yield return new TextStartStreamPart("text");
            while (Interlocked.Increment(ref emitted) <= 1000)
            {
                yield return new TextDeltaStreamPart("text", "hello");
                await Task.Yield();
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "allows metadata to be requested before, during, and after consumption without claiming a stream"
    )]
    public async Task Allows_metadata_to_be_requested_before_during_and_after_consumption_without_claiming_a_stream()
    {
        var result = LowMemory(TextParts());
        var usageBefore = result.Usage;
        var reasonBefore = result.FinishReason;

        var values = new List<string>();
        await foreach (var delta in result.TextStream())
        {
            values.Add(delta);
            var usageDuring = result.Usage;
            Assert.Same(usageBefore, usageDuring);
        }

        Assert.Equal(new[] { "hello" }, values);
        Assert.Equal(FinishReason.Stop, await reasonBefore);
        var usage = await result.Usage;
        Assert.Equal(3, usage.InputTokens);
        Assert.Equal(2, usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects a second projection without affecting the first")]
    public async Task Rejects_a_second_projection_without_affecting_the_first()
    {
        var result = LowMemory(TextParts());
        var stream = result.TextStream();

        var error = Assert.Throws<AiSdkException>(() => result.Stream());
        Assert.Contains("one stream consumer", error.Message);
        Assert.Equal(new[] { "hello" }, await ToList(stream));
        error = Assert.Throws<AiSdkException>(() => result.TextStream());
        Assert.Contains("one stream consumer", error.Message);
        Assert.Equal(FinishReason.Stop, await result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "requires an explicit output specification for cumulative partial output"
    )]
    public async Task Requires_an_explicit_output_specification_for_cumulative_partial_output()
    {
        var result = LowMemory(TextParts());

        var error = Assert.Throws<UnsupportedFunctionalityException>(() =>
            result.PartialOutputStream()
        );
        Assert.Contains("explicit output specification", error.Message);
        Assert.Equal(new[] { "hello" }, await ToList(result.TextStream()));
    }

    [Fact]
    [UpstreamTest(Prefix + "supports cumulative text partials when explicitly requested")]
    public async Task Supports_cumulative_text_partials_when_explicitly_requested()
    {
        var result = LowMemory(
            [
                new TextStartStreamPart("text"),
                new TextDeltaStreamPart("text", "a"),
                new TextDeltaStreamPart("text", "b"),
                new TextEndStreamPart("text"),
                Finish(),
            ],
            OutputSpec.Text()
        );

        var partials = await ToList(result.PartialOutputStream());

        Assert.Equal(new[] { "a", "ab" }, partials.Select(p => p.GetString()));
    }

    [Fact]
    [UpstreamTest(Prefix + "retains content required by global telemetry")]
    public async Task Retains_content_required_by_global_telemetry()
    {
        var previous = TelemetryRegistry.Integrations;
        var ends = new System.Collections.Concurrent.ConcurrentBag<GenerateTextResult>();
        TelemetryRegistry.Integrations =
        [
            new TestTelemetry
            {
                OnEnd = (e, _) =>
                {
                    ends.Add((GenerateTextResult)e);
                    return Task.CompletedTask;
                },
            },
        ];
        try
        {
            var text = "hello-" + Guid.NewGuid().ToString("N");
            var result = LowMemory(TextParts(text));
            await result.ConsumeStreamAsync();
            // The text is unique to this call, so a concurrent test cannot satisfy the assertion.
            Assert.Contains(ends, end => end.Text == text);
        }
        finally
        {
            TelemetryRegistry.Integrations = previous;
        }
    }
}
