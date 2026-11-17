// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; array output &gt; array with 2 elements streamed in 1 chunk</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputArrayOutputArrayWith2ElementsStreamedIn1ChunkTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > array output > array with 2 elements streamed in 1 chunk::";

    private static StreamTextResult Create()
    {
        return OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text(
                "{\"elements\":[{\"content\":\"element 1\"},{\"content\":\"element 2\"}]}"
            ),
            OutputSpec.Array(OptionsOutputSupport.ContentSchema)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream only complete objects in partialObjectStream")]
    public async Task Should_stream_only_complete_objects_in_partialObjectStream()
    {
        Assert.Equal(
            new[] { "[{\"content\":\"element 1\"},{\"content\":\"element 2\"}]" },
            await OptionsOutputSupport.Collect(Create())
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream individual elements in elementStream")]
    public async Task Should_stream_individual_elements_in_elementStream()
    {
        Assert.Equal(
            new[] { "{\"content\":\"element 1\"}", "{\"content\":\"element 2\"}" },
            await OptionsOutputSupport.CollectElements(Create())
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve output promise with the correct content")]
    public async Task Should_resolve_output_promise_with_the_correct_content()
    {
        var output = await Create().Output;

        Assert.Equal(
            """[{"content":"element 1"},{"content":"element 2"}]""",
            output!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve text promise with the correct text")]
    public async Task Should_resolve_text_promise_with_the_correct_text()
    {
        Assert.Equal(
            """{"elements":[{"content":"element 1"},{"content":"element 2"}]}""",
            await Create().Text
        );
    }
}
