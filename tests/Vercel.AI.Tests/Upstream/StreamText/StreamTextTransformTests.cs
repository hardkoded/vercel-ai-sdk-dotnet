// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using static Vercel.AI.Tests.Upstream.StreamText.StreamTextTransformTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>options.transform</c> (the stream-level tests).</summary>
public sealed class StreamTextTransformTests
{
    private const string Base =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.transform > ";

    private static StreamTextTransform UpperCase =>
        _ =>
            Map(chunk =>
                chunk is TextDeltaPart delta
                    ? [new TextDeltaPart(delta.Id, delta.Text.ToUpperInvariant(), delta.ProviderMetadata)]
                    : [chunk]
            );

    [Fact]
    [UpstreamTest(
        Base + "with base transformation::should transform the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_the_stream()
    {
        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    ExperimentalTransform = [UpperCase],
                    Prompt = "test-input",
                }
            );

        Assert.Equal(["HELLO", ", ", "WORLD!"], await TextStreamAsync(result));
    }

    [Fact]
    [UpstreamTest(
        Base + "with transformation that reads tools::should pass an empty tool set when no tools are provided",
        Coverage = UpstreamCoverage.Covered,
        Commit = "485e2a05eb8b2820694afc1c5d227631721fdfba"
    )]
    public async Task Should_pass_an_empty_tool_set_when_no_tools_are_provided()
    {
        IReadOnlyList<Tool>? receivedTools = null;

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    ExperimentalTransform =
                    [
                        options =>
                        {
                            receivedTools = options.Tools;
                            return Map(chunk => [chunk]);
                        },
                    ],
                    Prompt = "test-input",
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Empty(receivedTools!);
    }

    [Fact]
    [UpstreamTest(
        Base + "with multiple transformations::should transform the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_the_stream_with_multiple_transformations()
    {
        StreamTextTransform toUppercaseAndAddComma =
            _ =>
                Map(chunk =>
                    chunk is TextDeltaPart delta
                        ? [new TextDeltaPart(delta.Id, delta.Text.ToUpperInvariant() + ",", delta.ProviderMetadata)]
                        : [chunk]
                );
        StreamTextTransform omitComma =
            _ =>
                Map(chunk =>
                    chunk is TextDeltaPart delta
                        ? [new TextDeltaPart(delta.Id, delta.Text.Replace(",", ""), delta.ProviderMetadata)]
                        : [chunk]
                );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = CreateTestModel(),
                    ExperimentalTransform = [toUppercaseAndAddComma, omitComma],
                    Prompt = "test-input",
                }
            );

        Assert.Equal(["HELLO", " ", "WORLD!"], await TextStreamAsync(result));
    }

    [Fact]
    [UpstreamTest(
        Base + "with transformation that aborts stream::stream should stop when STOP token is encountered",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the stream parts, not the exact finish-step and finish payloads."
    )]
    public async Task Partial_Stream_should_stop_when_STOP_token_is_encountered()
    {
        var model = SteeringTestModel.Stream(() =>
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello, "),
                new TextDeltaStreamPart("1", "STOP"),
                new TextDeltaStreamPart("1", " World"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, SteeringTestSupport.Usage, "stop"),
            }
        );
        StreamTextTransform stopWord =
            options =>
                Map(chunk =>
                {
                    if (chunk is not TextDeltaPart delta)
                    {
                        return [chunk];
                    }

                    if (delta.Text.Contains("STOP"))
                    {
                        options.StopStream();
                        return [new FinishPart(FinishReason.Stop, LanguageModelUsage.Empty)];
                    }

                    return [chunk];
                });

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = "test-input",
                    ExperimentalTransform = [stopWord],
                }
            );

        var types = new List<string>();
        await foreach (var part in result.Stream())
        {
            types.Add(part.Type);
        }

        Assert.Equal(["start", "start-step", "text-start", "text-delta", "finish"], types);
    }

    private static AiClient Client() => SteeringTestSupport.Client();
}
