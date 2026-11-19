// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamTextWithMinimalRetention;

/// <summary>Port of <c>stream-text-with-minimal-retention.test.ts</c> &gt; <c>streaming-only contract</c>. Only the tests below are ported.</summary>
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
}
