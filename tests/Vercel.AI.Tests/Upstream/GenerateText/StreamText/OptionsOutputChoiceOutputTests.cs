// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; options.output &gt; choice output</c>. Only the tests below are ported.</summary>
public sealed class OptionsOutputChoiceOutputTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.output > choice output::";

    [Fact]
    [UpstreamTest(Prefix + "should stream an choice value")]
    public async Task Should_stream_an_choice_value()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"result\": ", "\"su", "nny", "\"", " }"),
            OutputSpec.Choice(new[] { "sunny", "rainy", "snowy" })
        );

        Assert.Equal(new[] { "\"sunny\"" }, await OptionsOutputSupport.Collect(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve text promise with the correct text")]
    public async Task Should_resolve_text_promise_with_the_correct_text()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"result\": ", "\"su", "nny", "\"", " }"),
            OutputSpec.Choice(new[] { "sunny", "rainy", "snowy" })
        );

        Assert.Equal("{ \"result\": \"sunny\" }", await result.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve output promise with the correct content")]
    public async Task Should_resolve_output_promise_with_the_correct_content()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"result\": ", "\"su", "nny", "\"", " }"),
            OutputSpec.Choice(new[] { "sunny", "rainy", "snowy" })
        );

        Assert.Equal("sunny", (await result.Output)!.Value.GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should not stream incorrect values")]
    public async Task Should_not_stream_incorrect_values()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"result\": ", "\"foo", "bar", "\"", " }"),
            OutputSpec.Choice(new[] { "sunny", "rainy", "snowy" })
        );

        Assert.Empty(await OptionsOutputSupport.Collect(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle ambiguous values")]
    public async Task Should_handle_ambiguous_values()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.TextWithoutEnd("{ ", "\"result\": ", "\"foo", "bar", "\"", " }"),
            OutputSpec.Choice(new[] { "foobar", "foobar2" })
        );

        Assert.Equal(new[] { "\"foobar\"" }, await OptionsOutputSupport.Collect(result));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle non-ambiguous values")]
    public async Task Should_handle_non_ambiguous_values()
    {
        var result = OptionsOutputSupport.Stream(
            OptionsOutputSupport.Text("{ ", "\"result\": ", "\"foo", "bar", "\"", " }"),
            OutputSpec.Choice(new[] { "foobar", "barfoo" })
        );

        Assert.Equal(new[] { "\"foobar\"" }, await OptionsOutputSupport.Collect(result));
    }
}
