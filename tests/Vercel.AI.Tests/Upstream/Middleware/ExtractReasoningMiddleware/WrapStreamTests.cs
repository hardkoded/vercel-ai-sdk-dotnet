// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Reasoning = global::Vercel.AI.ExtractReasoningMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.ExtractReasoningMiddleware;

/// <summary>Port of <c>extract-reasoning-middleware.test.ts</c> &gt; <c>extractReasoningMiddleware &gt; wrapStream</c>.</summary>
public sealed class WrapStreamTests
{
    private const string Id =
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > wrapStream::";

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static LanguageModelStreamPart Metadata() =>
        new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );

    private static LanguageModelStreamPart Finish() =>
        new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(5, 10, 15), "stop");

    private static TestLanguageModel ModelOf(params LanguageModelStreamPart[] parts) =>
        new() { StreamParts = parts };

    private static StreamTextResult Stream(
        TestLanguageModel model,
        Reasoning middleware,
        string prompt = "Hello, how can I help?"
    ) =>
        Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model.WrapLanguageModel(middleware),
                    Prompt = prompt,
                }
            );

    private static async Task<List<TextStreamPart>> Collect(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    // One line per part, so a whole stream compares in a single assertion.
    private static string Describe(TextStreamPart part) =>
        part switch
        {
            StartPart => "start",
            StartStepPart => "start-step",
            ReasoningStartPart p => $"reasoning-start {p.Id}",
            ReasoningDeltaPart p => $"reasoning-delta {p.Id} {p.Text}",
            ReasoningEndPart p => $"reasoning-end {p.Id}",
            TextStartPart p => $"text-start {p.Id}",
            TextDeltaPart p => $"text-delta {p.Id} {p.Text}",
            TextEndPart p => $"text-end {p.Id}",
            StepFinishPart => "finish-step",
            FinishPart => "finish",
            _ => part.GetType().Name,
        };

    private static void AssertStream(IReadOnlyList<TextStreamPart> parts, string[] expected)
    {
        Assert.Equal(expected, parts.Select(Describe).ToArray());

        var finishStep = Assert.Single(parts.OfType<StepFinishPart>());
        Assert.Equal(FinishReason.Stop, finishStep.Step.FinishReason);
        Assert.Equal("stop", finishStep.RawFinishReason);
        Assert.Equal(5, finishStep.Step.Usage.InputTokens);
        Assert.Equal(10, finishStep.Step.Usage.OutputTokens);
        Assert.Equal(15, finishStep.Step.Usage.TotalTokens);
        Assert.Equal("id-0", finishStep.Response.Id);
        Assert.Equal("mock-model-id", finishStep.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), finishStep.Response.Timestamp);

        var finish = Assert.Single(parts.OfType<FinishPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(5, finish.Usage.InputTokens);
        Assert.Equal(10, finish.Usage.OutputTokens);
        Assert.Equal(15, finish.Usage.TotalTokens);
    }

    private static LanguageModelStreamPart[] Chunks(params string[] deltas) =>
        new LanguageModelStreamPart[] { Metadata(), new TextStartStreamPart("1") }
            .Concat(deltas.Select(d => (LanguageModelStreamPart)new TextDeltaStreamPart("1", d)))
            .Concat(new LanguageModelStreamPart[] { new TextEndStreamPart("1"), Finish() })
            .ToArray();

    [Fact]
    [UpstreamTest(
        Id + "should not read Object.prototype for missing text part ids",
        Coverage = UpstreamCoverage.Partial,
        Note = "JavaScript Object.prototype pollution has no .NET equivalent. Checks that a delta whose id has no text-start is streamed as text and raises no error."
    )]
    public async Task Should_not_read_Objectprototype_for_missing_text_part_ids()
    {
        var model = ModelOf(Metadata(), new TextDeltaStreamPart("__proto__", "Hello"), Finish());

        var parts = await Collect(Stream(model, new Reasoning("think")));

        Assert.Empty(parts.OfType<ErrorPart>());
        Assert.Equal(
            "Hello",
            string.Concat(parts.OfType<TextDeltaPart>().Select(part => part.Text))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should extract reasoning from split <think> tags",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_from_split_think_tags()
    {
        var model = ModelOf(
            Chunks("<think>", "ana", "lyzing the request", "</think>", "Here", " is the response")
        );

        var parts = await Collect(Stream(model, new Reasoning("think")));

        AssertStream(
            parts,
            [
                "start",
                "start-step",
                "reasoning-start reasoning-0",
                "reasoning-delta reasoning-0 ana",
                "reasoning-delta reasoning-0 lyzing the request",
                "reasoning-end reasoning-0",
                "text-start 1",
                "text-delta 1 Here",
                "text-delta 1  is the response",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should extract reasoning from single chunk with multiple <think> tags",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_from_single_chunk_with_multiple_think_tags()
    {
        var model = ModelOf(
            Chunks(
                "<think>analyzing the request</think>Here is the response<think>thinking about the response</think>more"
            )
        );

        var parts = await Collect(Stream(model, new Reasoning("think")));

        AssertStream(
            parts,
            [
                "start",
                "start-step",
                "reasoning-start reasoning-0",
                "reasoning-delta reasoning-0 analyzing the request",
                "reasoning-end reasoning-0",
                "text-start 1",
                "text-delta 1 Here is the response",
                "reasoning-start reasoning-1",
                "reasoning-delta reasoning-1 \nthinking about the response",
                "reasoning-end reasoning-1",
                "text-delta 1 \nmore",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should extract reasoning from <think> when there is no text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_reasoning_from_think_when_there_is_no_text()
    {
        var model = ModelOf(Chunks("<think>", "ana", "lyzing the request\n", "</think>"));

        var parts = await Collect(Stream(model, new Reasoning("think")));

        AssertStream(
            parts,
            [
                "start",
                "start-step",
                "reasoning-start reasoning-0",
                "reasoning-delta reasoning-0 ana",
                "reasoning-delta reasoning-0 lyzing the request\n",
                "reasoning-end reasoning-0",
                "text-start 1",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should prepend <think> tag if startWithReasoning is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prepend_think_tag_if_startWithReasoning_is_true()
    {
        var model = ModelOf(
            Chunks("ana", "lyzing the request\n", "</think>", "this is the response")
        );

        var resultTrue = await Collect(
            Stream(model, new Reasoning("think", startWithReasoning: true))
        );
        var resultFalse = await Collect(Stream(model, new Reasoning("think")));

        AssertStream(
            resultTrue,
            [
                "start",
                "start-step",
                "reasoning-start reasoning-0",
                "reasoning-delta reasoning-0 ana",
                "reasoning-delta reasoning-0 lyzing the request\n",
                "reasoning-end reasoning-0",
                "text-start 1",
                "text-delta 1 this is the response",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
        AssertStream(
            resultFalse,
            [
                "start",
                "start-step",
                "text-start 1",
                "text-delta 1 ana",
                "text-delta 1 lyzing the request\n",
                "text-delta 1 </think>",
                "text-delta 1 this is the response",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should keep original text when <think> tag is not present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_original_text_when_think_tag_is_not_present()
    {
        var model = ModelOf(Chunks("this is the response"));

        var parts = await Collect(Stream(model, new Reasoning("think")));

        AssertStream(
            parts,
            [
                "start",
                "start-step",
                "text-start 1",
                "text-delta 1 this is the response",
                "text-end 1",
                "finish-step",
                "finish",
            ]
        );
    }

    [Theory]
    [InlineData("Use the <th", "Use the <th", null, "text")]
    [InlineData("<think>a </th", "", "a </th", "reasoning")]
    [UpstreamTest(
        Id + "should preserve a partial tag in $location when the text part ends",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_a_partial_tag_in_location_when_the_text_part_ends(
        string delta,
        string expectedText,
        string? expectedReasoningText,
        string location
    )
    {
        Assert.NotEmpty(location);
        var model = ModelOf(
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", delta),
            new TextEndStreamPart("1"),
            Finish()
        );

        var result = Stream(model, new Reasoning("think"), "Test prompt");

        Assert.Equal(expectedText, await result.Text);
        Assert.Equal(expectedReasoningText, await result.ReasoningText);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve overlapping text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_overlapping_text_parts()
    {
        var model = ModelOf(
            Metadata(),
            new TextStartStreamPart("a"),
            new TextStartStreamPart("b"),
            new TextDeltaStreamPart("a", "Alpha."),
            new TextDeltaStreamPart("b", "Beta."),
            new TextEndStreamPart("a"),
            new TextEndStreamPart("b"),
            Finish()
        );

        var result = Stream(model, new Reasoning("think"));
        var parts = await Collect(result);

        Assert.Equal("Alpha.Beta.", await result.Text);
        Assert.Empty(parts.OfType<ErrorPart>());
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve reasoning from overlapping text parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_reasoning_from_overlapping_text_parts()
    {
        var model = ModelOf(
            Metadata(),
            new TextStartStreamPart("a"),
            new TextStartStreamPart("b"),
            new TextDeltaStreamPart("a", "<think>A"),
            new TextDeltaStreamPart("b", "<think>B"),
            new TextDeltaStreamPart("a", "1</think>Alpha."),
            new TextDeltaStreamPart("b", "2</think>Beta."),
            new TextEndStreamPart("a"),
            new TextEndStreamPart("b"),
            Finish()
        );

        var result = Stream(model, new Reasoning("think"));
        var parts = await Collect(result);

        Assert.Equal("A1B2", await result.ReasoningText);
        Assert.Equal("Alpha.Beta.", await result.Text);
        Assert.Empty(parts.OfType<ErrorPart>());
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle empty <think></think> tags without crashing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_think_think_tags_without_crashing()
    {
        var model = ModelOf(Chunks("<think></think>", " This is the answer."));

        var parts = await Collect(Stream(model, new Reasoning("think"), "Test prompt"));

        var reasoningStartIndex = parts.FindIndex(part =>
            part is ReasoningStartPart { Id: "reasoning-0" }
        );
        var reasoningEndIndex = parts.FindIndex(part =>
            part is ReasoningEndPart { Id: "reasoning-0" }
        );
        Assert.True(reasoningStartIndex >= 0);
        Assert.True(reasoningEndIndex >= 0);
        Assert.True(reasoningEndIndex > reasoningStartIndex);
    }
}
