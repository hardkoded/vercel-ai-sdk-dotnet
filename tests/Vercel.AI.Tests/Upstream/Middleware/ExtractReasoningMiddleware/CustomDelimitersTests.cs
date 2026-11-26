// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using Reasoning = global::Vercel.AI.ExtractReasoningMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.ExtractReasoningMiddleware;

/// <summary>Port of <c>extract-reasoning-middleware.test.ts</c> &gt; <c>extractReasoningMiddleware &gt; custom delimiters</c>.</summary>
public sealed class CustomDelimitersTests
{
    private const string Id =
        "packages/ai/src/middleware/extract-reasoning-middleware.test.ts::extractReasoningMiddleware > custom delimiters";

    private static readonly (string Opening, string Closing) GemmaTags = (
        "<|channel>thought\n",
        "<channel|>"
    );

    private static readonly Dictionary<string, Case> Cases = new()
    {
        ["Gemma thought channel"] = new(
            () => new Reasoning(GemmaTags),
            "<|channel>thought\nChecking the sum.\nIt is four.<channel|>4",
            "Checking the sum.\nIt is four.",
            "4"
        ),
        ["empty Gemma thought channel"] = new(
            () => new Reasoning(GemmaTags),
            "<|channel>thought\n<channel|>4",
            string.Empty,
            "4"
        ),
        ["omitted opening delimiter"] = new(
            () => new Reasoning(GemmaTags, startWithReasoning: true),
            "Checking the sum.<channel|>4",
            "Checking the sum.",
            "4"
        ),
        ["literal regex metacharacters"] = new(
            () => new Reasoning(("[.*+?^${}()|\\]", "(end.*+?^${}|\\)")),
            "[.*+?^${}()|\\]Checking the sum.(end.*+?^${}|\\)4",
            "Checking the sum.",
            "4"
        ),
        ["multiple reasoning blocks and a custom separator"] = new(
            () => new Reasoning(GemmaTags, separator: " / "),
            "Before<|channel>thought\nFirst<channel|>Between<|channel>thought\nSecond<channel|>After",
            "First / Second",
            "Before / Between / After"
        ),
        ["text without delimiters"] = new(() => new Reasoning(GemmaTags), "4", null, "4"),
        ["literal regex metacharacters in a string tag name"] = new(
            () => new Reasoning("think|reason"),
            "<think|reason>Checking the sum.</think|reason>4",
            "Checking the sum.",
            "4"
        ),
    };

    public static TheoryData<string> CaseNames => new(Cases.Keys);

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    [Theory]
    [MemberData(nameof(CaseNames))]
    [UpstreamTest(
        Id + " > $name::extracts reasoning in generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Extracts_reasoning_in_generateText(string name)
    {
        var testCase = Cases[name];
        var model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text(testCase.Input),
        }.WrapLanguageModel(testCase.Middleware());

        var result = await Client()
            .GenerateTextAsync(
                new GenerateTextOptions { Model = model, Prompt = "What is 2 + 2?" }
            );

        Assert.Equal(testCase.Text, result.Text);
        Assert.Equal(
            testCase.Reasoning == string.Empty ? null : testCase.Reasoning,
            result.ReasoningText
        );
        if (testCase.Reasoning == string.Empty)
        {
            var reasoning = Assert.Single(result.Content.OfType<GeneratedReasoning>());
            Assert.Equal(string.Empty, reasoning.Text);
        }
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    [UpstreamTest(
        Id + " > $name::extracts reasoning when every delimiter character is streamed separately",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Extracts_reasoning_when_every_delimiter_character_is_streamed_separately(
        string name
    )
    {
        var testCase = Cases[name];
        var streamParts = new List<LanguageModelStreamPart> { new TextStartStreamPart("text-0") };
        streamParts.AddRange(
            testCase.Input.Select(delta => new TextDeltaStreamPart("text-0", delta.ToString()))
        );
        streamParts.Add(new TextEndStreamPart("text-0"));
        streamParts.Add(
            new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(5, 10, 15), "stop")
        );
        var model = new TestLanguageModel { StreamParts = streamParts }.WrapLanguageModel(
            testCase.Middleware()
        );

        var result = Client()
            .StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "What is 2 + 2?" });
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Empty(parts.OfType<ErrorPart>());
        Assert.Equal(testCase.Text, await result.Text);
        Assert.Equal(
            testCase.Reasoning == string.Empty ? null : testCase.Reasoning,
            (await result.Steps)[^1].ReasoningText
        );
        if (testCase.Reasoning != null)
        {
            var starts = parts.OfType<ReasoningStartPart>().Select(part => part.Id).ToList();
            var ends = parts.OfType<ReasoningEndPart>().Select(part => part.Id).ToList();
            Assert.NotEmpty(starts);
            Assert.Equal(starts, ends);
        }
    }

    [Theory]
    [InlineData("", "<channel|>")]
    [InlineData("<|channel>thought\n", "")]
    [UpstreamTest(Id + "::rejects empty delimiters: %j", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_empty_delimiters(string opening, string closing)
    {
        var error = Assert.Throws<InvalidArgumentError>(() => new Reasoning((opening, closing)));
        Assert.Equal("tagName", error.Parameter);
        Assert.Contains("Reasoning delimiters must not be empty.", error.Message);
    }

    private sealed record Case(
        Func<Reasoning> Middleware,
        string Input,
        string? Reasoning,
        string Text
    );
}
