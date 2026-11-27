// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Citations;

/// <summary>Port of <c>citations.test.ts</c> &gt; <c>citations</c>.</summary>
public sealed class CitationsTests
{
    private static readonly GeneratedSource CitedSource = new(
        "cited",
        "https://example.com/cited",
        "Cited reference"
    );

    private static readonly GeneratedDocumentSource DocumentSource = new(
        "document",
        "application/pdf",
        "Report",
        "report.pdf",
        JsonDocument
            .Parse("{\"anthropic\":{\"start_page_number\":1,\"end_page_number\":2}}")
            .RootElement.Clone()
    );

    private static readonly List<Citation> Cited = new()
    {
        new Citation(CitedSource, startIndex: 0, endIndex: 6),
        new Citation(DocumentSource, citedText: "Supporting passage"),
    };

    private static readonly GeneratedSource RetrievedSource = new(
        "retrieved",
        "https://example.com/retrieved",
        null
    );

    private static readonly LanguageModelUsage Usage = new(1, 1, 2);

    [Theory]
    [InlineData("Answer")]
    [InlineData("")]
    [UpstreamTest(
        "packages/ai/src/generate-text/citations.test.ts::citations::preserves citations when simulating streaming for text %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_citations_when_simulating_streaming_for_text(string text)
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText(text, Cited) },
                FinishReason.Stop,
                Usage,
                "stop"
            ),
        };

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model.WrapLanguageModel(new SimulateStreamingMiddleware()),
                    Prompt = "Question",
                }
            );
        await result.Text;

        var steps = await result.Steps;
        var part = Assert.IsAssignableFrom<GeneratedText>(
            Assert.Single(steps[^1].Content, content => content is GeneratedText)
        );
        Assert.Equal(text, part.Text);
        Assert.Equal(Cited, part.Citations);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/citations.test.ts::citations::preserves citations on generated text independently of retrieved sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_citations_on_generated_text_independently_of_retrieved_sources()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { RetrievedSource, new GeneratedText("Answer", Cited) },
                FinishReason.Stop,
                Usage,
                "stop"
            ),
        };

        var result = await Client()
            .GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Question" });

        var text = Assert.IsAssignableFrom<GeneratedText>(
            Assert.Single(result.Content, part => part is GeneratedText)
        );
        Assert.Equal("Answer", text.Text);
        Assert.Equal(Cited, text.Citations);
        Assert.Equal(new[] { RetrievedSource }, result.Sources);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/citations.test.ts::citations::preserves text-end citations in core results and validated UI messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_text_end_citations_in_core_results_and_validated_UI_messages()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                RetrievedSourcePart,
                new TextStartStreamPart("text"),
                new TextDeltaStreamPart("text", "Answer"),
                new TextEndStreamPart("text", Cited),
                new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
            },
        };

        var result = Client()
            .StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "Question" });
        var messages = await ReadMessagesAsync(result);
        var finalMessage = messages[^1];

        var uiText = Assert.IsType<TextUIPart>(
            Assert.Single(finalMessage.Parts, part => part is TextUIPart)
        );
        Assert.Equal("Answer", uiText.Text);
        AssertCitationsEqual(Cited, uiText.Citations);

        var content = await result.Content;
        var text = Assert.IsAssignableFrom<GeneratedText>(
            Assert.Single(content, part => part is GeneratedText)
        );
        Assert.Equal("Answer", text.Text);
        AssertCitationsEqual(Cited, text.Citations);
        var sources = Assert.Single(await result.Sources);
        Assert.Equal(RetrievedSource.Id, sources.Id);
        Assert.Equal(RetrievedSource.Url, sources.Url);

        var validated = await Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions { Messages = new[] { finalMessage } }
        );
        Assert.True(
            JsonNode.DeepEquals(
                UIMessageJson.ToJson(finalMessage),
                UIMessageJson.ToJson(Assert.Single(validated))
            )
        );

        // Output citations are presentation data; a follow-up sends the text and
        // provider metadata, without injecting normalized references into the prompt.
        var modelMessages = Ai.ConvertToModelMessages(new[] { finalMessage });
        var assistant = Assert.IsType<AssistantModelMessage>(Assert.Single(modelMessages));
        Assert.Equal("assistant", assistant.Role);
        var part = Assert.IsType<AssistantTextPart>(Assert.Single(assistant.Parts));
        Assert.Equal("Answer", part.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/citations.test.ts::citations::rejects malformed citations in persisted UI messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_malformed_citations_in_persisted_UI_messages()
    {
        using var document = JsonDocument.Parse(
            """
            [{"id":"message","role":"assistant","parts":[{"type":"text","text":"Answer","citations":[{"source":{"type":"source","sourceType":"url","id":"citation"}}]}]}]
            """
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            Ai.ValidateUIMessagesAsync(
                new ValidateUIMessagesOptions { MessagesJson = document.RootElement.Clone() }
            )
        );
    }

    [Theory]
    [InlineData("https://example.com/report.pdf?version=2026-10-01#page=12")]
    [InlineData("https://example.com/article?oldid=42#:~:text=caf%C3%A9")]
    [UpstreamTest(
        "packages/ai/src/generate-text/citations.test.ts::citations::preserves repeated citations, Unicode offsets, and URL %s through streaming and persistence",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_repeated_citations_Unicode_offsets_and_URL_through_streaming_and_persistence(
        string url
    )
    {
        const string text = "🧪 café — 東京 café";
        // These mock provider offsets count Unicode code points rather than UTF-16 code
        // units. The SDK must preserve them as supplied.
        var textCitations = new List<Citation>
        {
            new(new GeneratedSource(url, url, null), startIndex: 2, endIndex: 6, citedText: "café"),
            new(
                new GeneratedSource(url, url, null),
                startIndex: 12,
                endIndex: 16,
                citedText: "café"
            ),
        };
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(null),
                new TextStartStreamPart("text"),
                new TextDeltaStreamPart("text", "🧪 café — "),
                new TextDeltaStreamPart("text", "東京 café"),
                new TextEndStreamPart("text", textCitations),
                new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
            },
        };

        var result = Client()
            .StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "Question" });
        var messages = await ReadMessagesAsync(result);

        var content = await result.Content;
        var generated = Assert.IsAssignableFrom<GeneratedText>(
            Assert.Single(content, part => part is GeneratedText)
        );
        Assert.Equal(text, generated.Text);
        AssertCitationsEqual(textCitations, generated.Citations);

        using var persisted = JsonDocument.Parse(
            new JsonArray(UIMessageJson.ToJson(messages[^1])).ToJsonString()
        );
        var validated = await Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions { MessagesJson = persisted.RootElement.Clone() }
        );
        var persistedText = Assert.IsType<TextUIPart>(
            Assert.Single(Assert.Single(validated).Parts, part => part is TextUIPart)
        );
        Assert.Equal(text, persistedText.Text);
        AssertCitationsEqual(textCitations, persistedText.Citations);
    }

    private static readonly SourceStreamPart RetrievedSourcePart = new(
        "retrieved",
        "https://example.com/retrieved",
        null
    );

    private static async Task<List<UIMessage>> ReadMessagesAsync(StreamTextResult result)
    {
        var messages = new List<UIMessage>();
        await foreach (
            var message in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(
                    result.ToUIMessageStream(new UIMessageStreamOptions { SendSources = true })
                )
            )
        )
        {
            messages.Add(message);
        }

        return messages;
    }

    private static void AssertCitationsEqual(
        IReadOnlyList<Citation> expected,
        IReadOnlyList<Citation>? actual
    )
    {
        Assert.NotNull(actual);
        Assert.True(
            JsonNode.DeepEquals(CitationJson.ToJson(expected), CitationJson.ToJson(actual)),
            "Citations differ."
        );
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
