// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Middleware;

public sealed class ExtractReasoningMiddlewareTests
{
    private static readonly (string Opening, string Closing) GemmaTags = ("<|channel>thought\n", "<channel|>");

    private static readonly Dictionary<string, Case> Cases = new()
    {
        ["Gemma thought channel"] = new(
            () => new ExtractReasoningMiddleware(GemmaTags),
            "<|channel>thought\nChecking the sum.\nIt is four.<channel|>4",
            "Checking the sum.\nIt is four.",
            "4"),
        ["empty Gemma thought channel"] = new(
            () => new ExtractReasoningMiddleware(GemmaTags),
            "<|channel>thought\n<channel|>4",
            string.Empty,
            "4"),
        ["omitted opening delimiter"] = new(
            () => new ExtractReasoningMiddleware(GemmaTags, startWithReasoning: true),
            "Checking the sum.<channel|>4",
            "Checking the sum.",
            "4"),
        ["literal regex metacharacters"] = new(
            () => new ExtractReasoningMiddleware(("[.*+?^${}()|\\]", "(end.*+?^${}|\\)")),
            "[.*+?^${}()|\\]Checking the sum.(end.*+?^${}|\\)4",
            "Checking the sum.",
            "4"),
        ["multiple reasoning blocks and a custom separator"] = new(
            () => new ExtractReasoningMiddleware(GemmaTags, separator: " / "),
            "Before<|channel>thought\nFirst<channel|>Between<|channel>thought\nSecond<channel|>After",
            "First / Second",
            "Before / Between / After"),
        ["text without delimiters"] = new(
            () => new ExtractReasoningMiddleware(GemmaTags),
            "4",
            null,
            "4"),
        ["literal regex metacharacters in a string tag name"] = new(
            () => new ExtractReasoningMiddleware("think|reason"),
            "<think|reason>Checking the sum.</think|reason>4",
            "Checking the sum.",
            "4"),
    };

    public static TheoryData<string> CaseNames => new(Cases.Keys);

    [Theory]
    [MemberData(nameof(CaseNames))]
    public async Task ExtractsReasoningInGenerateText(string name)
    {
        var testCase = Cases[name];
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text(testCase.Input) }
            .WrapLanguageModel(testCase.Middleware());

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "What is 2 + 2?" });

        Assert.Equal(testCase.Text, result.Text);
        Assert.Equal(testCase.Reasoning == string.Empty ? null : testCase.Reasoning, result.ReasoningText);
        if (testCase.Reasoning == string.Empty)
        {
            var generated = await model.DoGenerateAsync(new LanguageModelCallOptions(), CancellationToken.None);
            var reasoning = Assert.Single(generated.Content.OfType<GeneratedReasoning>());
            Assert.Equal(string.Empty, reasoning.Text);
        }
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public async Task ExtractsReasoningWhenEveryDelimiterCharacterIsStreamedSeparately(string name)
    {
        var testCase = Cases[name];
        var streamParts = new List<LanguageModelStreamPart> { new TextStartStreamPart("text-0") };
        streamParts.AddRange(testCase.Input.Select(delta => new TextDeltaStreamPart("text-0", delta.ToString())));
        streamParts.Add(new TextEndStreamPart("text-0"));
        streamParts.Add(new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty, "stop"));
        var model = new TestLanguageModel { StreamParts = streamParts }.WrapLanguageModel(testCase.Middleware());

        var result = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "What is 2 + 2?" });
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        Assert.Empty(parts.OfType<ErrorPart>());
        Assert.Equal(testCase.Text, await result.Text);
        Assert.Equal(testCase.Reasoning == string.Empty ? null : testCase.Reasoning, (await result.Steps)[^1].ReasoningText);
        if (testCase.Reasoning != null)
        {
            var modelParts = new List<LanguageModelStreamPart>();
            await foreach (var part in model.DoStreamAsync(new LanguageModelCallOptions(), CancellationToken.None))
            {
                modelParts.Add(part);
            }

            var starts = modelParts.OfType<ReasoningStartStreamPart>().Select(part => part.Id).ToList();
            Assert.NotEmpty(starts);
            Assert.Equal(starts, modelParts.OfType<ReasoningEndStreamPart>().Select(part => part.Id));
        }
    }

    [Theory]
    [InlineData("", "<channel|>")]
    [InlineData("<|channel>thought\n", "")]
    public void RejectsEmptyDelimiters(string opening, string closing)
    {
        var error = Assert.Throws<InvalidArgumentError>(() => new ExtractReasoningMiddleware((opening, closing)));
        Assert.Equal("tagName", error.Parameter);
        Assert.Contains("Reasoning delimiters must not be empty.", error.Message);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    private sealed record Case(Func<ExtractReasoningMiddleware> Middleware, string Input, string? Reasoning, string Text);
}
