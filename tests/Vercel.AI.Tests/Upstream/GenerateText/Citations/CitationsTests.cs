// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Citations;

/// <summary>Port of <c>citations.test.ts</c> &gt; <c>citations</c>.</summary>
public sealed class CitationsTests
{
    private static readonly GeneratedSource CitedSource = new("cited", "https://example.com/cited", "Cited reference");

    private static readonly GeneratedDocumentSource DocumentSource = new(
        "document",
        "application/pdf",
        "Report",
        "report.pdf",
        JsonDocument.Parse("{\"anthropic\":{\"start_page_number\":1,\"end_page_number\":2}}").RootElement.Clone());

    private static readonly List<Citation> Cited = new()
    {
        new Citation(CitedSource, startIndex: 0, endIndex: 6),
        new Citation(DocumentSource, citedText: "Supporting passage"),
    };

    private static readonly GeneratedSource RetrievedSource = new("retrieved", "https://example.com/retrieved", null);

    private static readonly LanguageModelUsage Usage = new(1, 1, 2);

    [Theory]
    [InlineData("Answer")]
    [InlineData("")]
    [UpstreamTest("packages/ai/src/generate-text/citations.test.ts::citations::preserves citations when simulating streaming for text %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_citations_when_simulating_streaming_for_text(string text)
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText(text, Cited) },
                FinishReason.Stop,
                Usage,
                "stop"),
        };

        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = model.WrapLanguageModel(new SimulateStreamingMiddleware()),
            Prompt = "Question",
        });
        await result.Text;

        var steps = await result.Steps;
        var part = Assert.IsAssignableFrom<GeneratedText>(Assert.Single(steps[^1].Content, content => content is GeneratedText));
        Assert.Equal(text, part.Text);
        Assert.Equal(Cited, part.Citations);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/citations.test.ts::citations::preserves citations on generated text independently of retrieved sources", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_citations_on_generated_text_independently_of_retrieved_sources()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { RetrievedSource, new GeneratedText("Answer", Cited) },
                FinishReason.Stop,
                Usage,
                "stop"),
        };

        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Question" });

        var text = Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content, part => part is GeneratedText));
        Assert.Equal("Answer", text.Text);
        Assert.Equal(Cited, text.Citations);
        Assert.Equal(new[] { RetrievedSource }, result.Sources);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
