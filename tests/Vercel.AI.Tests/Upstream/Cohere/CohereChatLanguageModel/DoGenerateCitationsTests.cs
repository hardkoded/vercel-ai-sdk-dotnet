// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>citations</c>.</summary>
public sealed class DoGenerateCitationsTests
{
    private const string UpstreamCommit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f";
    private const string Chat =
        "packages/cohere/src/cohere-chat-language-model.test.ts::doGenerate > citations::";

    private static LanguageModelCallOptions Files(string text, params FileContentPart[] files)
    {
        var parts = new List<UserContentPart> { new TextContentPart(text) };
        parts.AddRange(files);
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(parts) },
        };
    }

    private static FileContentPart File(string mediaType, string content, string name) =>
        new(mediaType, null, Encoding.UTF8.GetBytes(content), name);

    private static async Task<JsonElement> RequestBody(LanguageModelCallOptions options)
    {
        var handler = JsonFixture("cohere-citations");
        await Model(handler).DoGenerateAsync(options, CancellationToken.None);
        return Json(handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(
        Chat + "should extract citations from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_citations_from_response()
    {
        var result = await Model(
                JsonFixture("cohere-citations"),
                "command-r-plus",
                Ids("test-citation-id")
            )
            .DoGenerateAsync(
                Files(
                    "What are AI benefits?",
                    File("text/plain", "AI provides automation and efficiency.", "ai-benefits.txt")
                ),
                CancellationToken.None
            );

        Assert.Equal(4, result.Content.Count);
        Assert.Equal(
            "The key benefits mentioned in this document are:\n1. Automation of tasks\n2. Better decision-making\n3. Cost reduction",
            Assert.IsType<GeneratedText>(result.Content[0]).Text
        );
        var sources = result
            .Content.Skip(1)
            .Select(part => Assert.IsType<GeneratedDocumentSource>(part))
            .ToList();
        Assert.All(sources, source => Assert.Equal("test-citation-id", source.Id));
        Assert.All(sources, source => Assert.Equal("text/plain", source.MediaType));
        Assert.All(sources, source => Assert.Equal("benefits.txt", source.Title));
        var first = sources[0].ProviderMetadata!.Value.GetProperty("cohere");
        Assert.Equal("TEXT_CONTENT", first.GetProperty("citationType").GetString());
        Assert.Equal(52, first.GetProperty("start").GetInt32());
        Assert.Equal("Automation of tasks", first.GetProperty("text").GetString());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(1683, result.Usage.InputTokens);
        Assert.Equal(62, result.Usage.OutputTokens);
        Assert.Equal(992, result.Usage.CacheReadTokens);
        Assert.Equal(691, result.Usage.NoCacheInputTokens);
        Assert.Equal("68475c80-574b-4c65-98a4-e81cebab5dce", result.ResponseId);
    }

    [Fact]
    [UpstreamTest(
        Chat + "should extract text documents and send to API",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_text_documents_and_send_to_API()
    {
        var body = await RequestBody(
            Files("What does this say?", File("text/plain", "This is a test document.", "test.txt"))
        );

        JsonAssert.Equal(
            body,
            "{\"documents\":[{\"data\":{\"text\":\"This is a test document.\",\"title\":\"test.txt\"}}],\"messages\":[{\"content\":\"What does this say?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Chat + "should extract multiple text documents",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_multiple_text_documents()
    {
        var body = await RequestBody(
            Files(
                "What do these documents say?",
                File("text/plain", "First document content", "doc1.txt"),
                File("text/plain", "Second document content", "doc2.txt")
            )
        );

        JsonAssert.Equal(
            body,
            "{\"documents\":[{\"data\":{\"text\":\"First document content\",\"title\":\"doc1.txt\"}},{\"data\":{\"text\":\"Second document content\",\"title\":\"doc2.txt\"}}],\"messages\":[{\"content\":\"What do these documents say?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}"
        );
    }

    [Fact]
    [UpstreamTest(Chat + "should support JSON files", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_support_JSON_files()
    {
        var body = await RequestBody(
            Files(
                "What is in this JSON?",
                File("application/json", "{\"key\": \"value\"}", "data.json")
            )
        );

        JsonAssert.Equal(
            body,
            "{\"documents\":[{\"data\":{\"text\":\"{\\\"key\\\": \\\"value\\\"}\",\"title\":\"data.json\"}}],\"messages\":[{\"content\":\"What is in this JSON?\",\"role\":\"user\"}],\"model\":\"command-r-plus\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Chat + "should not include mediaType in the outgoing payload (category D)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_mediaType_in_the_outgoing_payload_category_D()
    {
        var handler = JsonFixture("cohere-citations");
        await Model(handler)
            .DoGenerateAsync(
                Files(
                    "What is this?",
                    File("application/pdf", "Some file content", "document.pdf")
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Json(handler.Calls[0].Text).GetProperty("documents"),
            "[{\"data\":{\"text\":\"Some file content\",\"title\":\"document.pdf\"}}]"
        );
        Assert.DoesNotContain("application/pdf", handler.Calls[0].Text);
        Assert.DoesNotContain("mediaType", handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(
        Chat + "should successfully process supported text media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_successfully_process_supported_text_media_types()
    {
        var body = await RequestBody(
            Files(
                "What is this?",
                File("text/plain", "This is plain text content", "text.txt"),
                File("text/markdown", "# Markdown Header\nContent", "doc.md")
            )
        );

        JsonAssert.Equal(
            body.GetProperty("documents"),
            "[{\"data\":{\"text\":\"This is plain text content\",\"title\":\"text.txt\"}},{\"data\":{\"text\":\"# Markdown Header\\nContent\",\"title\":\"doc.md\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Chat + "should not include documents parameter when no files present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_documents_parameter_when_no_files_present()
    {
        Assert.False((await RequestBody(TestPrompt())).TryGetProperty("documents", out _));
    }

    [Fact]
    [UpstreamTest(
        Chat + "should map document citations without title and with missing fields",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Should_map_document_citations_without_title_and_with_missing_fields()
    {
        var handler = JsonBody(
            "{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Automation of tasks\"}],\"citations\":[{\"text\":\"Automation of tasks\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"AI provides automation of tasks.\",\"extra_field\":\"passed through\"}}],\"type\":\"TEXT_CONTENT\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"tokens\":{\"input_tokens\":10,\"output_tokens\":5}}}"
        );

        var result = await Model(handler, "command-r-plus", Ids("test-citation-id"))
            .DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            part => Assert.Equal("Automation of tasks", Assert.IsType<GeneratedText>(part).Text),
            part =>
                AssertSource(
                    part,
                    "test-citation-id",
                    "Document",
                    "{\"cohere\":{\"text\":\"Automation of tasks\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"AI provides automation of tasks.\",\"extra_field\":\"passed through\"}}],\"citationType\":\"TEXT_CONTENT\"}}"
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Chat + "should not emit sources for tool-sourced citations",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Should_not_emit_sources_for_tool_sourced_citations()
    {
        var handler = JsonBody(
            "{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"It is sunny in Berlin.\"}],\"citations\":[{\"start\":6,\"end\":11,\"text\":\"sunny\",\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"city\":\"Berlin\",\"condition\":\"sunny\"}}],\"type\":\"TEXT_CONTENT\"},{\"start\":0,\"end\":5,\"text\":\"It is\",\"sources\":null}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"tokens\":{\"input_tokens\":10,\"output_tokens\":5}}}"
        );

        var result = await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal(
            "It is sunny in Berlin.",
            Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text
        );
    }
}
