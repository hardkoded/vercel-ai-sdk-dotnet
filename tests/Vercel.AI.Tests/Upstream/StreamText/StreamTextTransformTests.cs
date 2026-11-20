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

    [Fact]
    [UpstreamTest(
        Base
            + "with transformation that reads tools::should pass an empty tool set when no tools are provided",
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
        StreamTextTransform toUppercaseAndAddComma = _ =>
            Map(chunk =>
                chunk is TextDeltaPart delta
                    ?
                    [
                        new TextDeltaPart(
                            delta.Id,
                            delta.Text.ToUpperInvariant() + ",",
                            delta.ProviderMetadata
                        ),
                    ]
                    : [chunk]
            );
        StreamTextTransform omitComma = _ =>
            Map(chunk =>
                chunk is TextDeltaPart delta
                    ?
                    [
                        new TextDeltaPart(
                            delta.Id,
                            delta.Text.Replace(",", ""),
                            delta.ProviderMetadata
                        ),
                    ]
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

    private static AiClient Client() => SteeringTestSupport.Client();
}
