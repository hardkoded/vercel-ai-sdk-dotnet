// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>citations</c>.</summary>
public sealed class DoStreamCitationsTests
{
    private const string UpstreamCommit = "033a980feebab0545dbc9a094fd0e1dd40ba8cf7";
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::doStream > citations::";
    private const string Citation = "{\"start\":0,\"end\":4,\"text\":\"text\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"text\",\"title\":\"test.txt\"}}],\"type\":\"TEXT_CONTENT\"}";
    private const string CitationMetadata = "{\"cohere\":{\"start\":0,\"end\":4,\"text\":\"text\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"text\",\"title\":\"test.txt\"}}],\"citationType\":\"TEXT_CONTENT\"}}";

    [Fact]
    [UpstreamTest(Chat + "should stream sources for citations", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_stream_sources_for_citations()
    {
        var model = Model(ChunksFixture("cohere-citations"), "command-a-03-2025", Ids("test-citation-id"));
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("What is the capital of Zephyria?"),
                    new FileContentPart("text/plain", null, System.Text.Encoding.UTF8.GetBytes("Zephyria is a fictional country. Its capital city is Marrowdale, a coastal city known for its lighthouses."), "zephyria.txt"),
                }),
            },
            IncludeRawChunks = false,
        };

        var parts = await Stream(model, options);

        AssertSource(
            Assert.Single(parts, part => part is DocumentSourceStreamPart),
            "test-citation-id",
            "zephyria.txt",
            "{\"cohere\":{\"start\":49,\"end\":60,\"text\":\"Marrowdale.\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"Zephyria is a fictional country. Its capital city is Marrowdale, a coastal city known for its lighthouses.\",\"title\":\"zephyria.txt\"}}],\"citationType\":\"TEXT_CONTENT\"}}");
    }

    [Fact]
    [UpstreamTest(Chat + "should stream a source for each citation in a citation-start event", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_stream_a_source_for_each_citation_in_a_citation_start_event()
    {
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":[" + Citation + "," + Citation + "]}}}",
            "{\"type\":\"citation-end\",\"index\":0}");

        var parts = await Stream(Model(handler, "command-a-03-2025", Ids("test-citation-id-1", "test-citation-id-2")), TestPrompt());

        Assert.Collection(
            parts.Where(part => part is DocumentSourceStreamPart),
            part => AssertSource(part, "test-citation-id-1", "test.txt", CitationMetadata),
            part => AssertSource(part, "test-citation-id-2", "test.txt", CitationMetadata));
    }

    [Fact]
    [UpstreamTest(Chat + "should tolerate citations without optional fields", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_tolerate_citations_without_optional_fields()
    {
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":{\"sources\":[{\"type\":\"document\",\"document\":{}}]}}}}",
            "{\"type\":\"citation-end\",\"index\":0}");

        var parts = await Stream(Model(handler, "command-a-03-2025", Ids("test-citation-id")), TestPrompt());

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        AssertSource(
            Assert.Single(parts, part => part is DocumentSourceStreamPart),
            "test-citation-id",
            "Document",
            "{\"cohere\":{\"sources\":[{\"type\":\"document\",\"document\":{}}]}}");
    }

    [Fact]
    [UpstreamTest(Chat + "should tolerate a citation-start event without citations", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_tolerate_a_citation_start_event_without_citations()
    {
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0}",
            "{\"type\":\"citation-end\",\"index\":0}",
            "{\"type\":\"message-end\",\"delta\":{\"finish_reason\":\"COMPLETE\",\"usage\":{\"tokens\":{\"input_tokens\":5,\"output_tokens\":2}}}}");

        var parts = await Stream(Model(handler, "command-a-03-2025"), TestPrompt());

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        Assert.DoesNotContain(parts, part => part is DocumentSourceStreamPart);
        Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(parts[^1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(Chat + "should skip tool-only citations", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_skip_tool_only_citations()
    {
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":{\"start\":6,\"end\":11,\"text\":\"sunny\",\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"city\":\"Berlin\",\"condition\":\"sunny\"}}],\"type\":\"TEXT_CONTENT\"}}}}",
            "{\"type\":\"citation-end\",\"index\":0}");

        var parts = await Stream(Model(handler, "command-a-03-2025"), TestPrompt());

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        Assert.DoesNotContain(parts, part => part is DocumentSourceStreamPart);
    }

    [Fact]
    [UpstreamTest(Chat + "should emit a source with a distinct id for each citation-start event", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_emit_a_source_with_a_distinct_id_for_each_citation_start_event()
    {
        const string citation = "{\"start\":49,\"end\":60,\"text\":\"Marrowdale.\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"Zephyria is a fictional country.\",\"title\":\"zephyria.txt\"}}],\"type\":\"TEXT_CONTENT\"}";
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":" + citation + "}}}",
            "{\"type\":\"citation-end\",\"index\":0}",
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":" + citation + "}}}",
            "{\"type\":\"citation-end\",\"index\":0}");

        var parts = await Stream(Model(handler, "command-a-03-2025", Ids("test-citation-id-1", "test-citation-id-2")), TestPrompt());

        var sources = parts.OfType<DocumentSourceStreamPart>().ToList();
        Assert.Equal(new[] { "test-citation-id-1", "test-citation-id-2" }, sources.Select(source => source.Id));
        Assert.All(sources, source =>
        {
            Assert.Equal("zephyria.txt", source.Title);
        });
    }

    [Fact]
    [UpstreamTest(Chat + "should use a fallback title for untitled documents", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_use_a_fallback_title_for_untitled_documents()
    {
        var handler = ChunkLines(
            "{\"type\":\"citation-start\",\"index\":0,\"delta\":{\"message\":{\"citations\":{\"start\":0,\"end\":4,\"text\":\"text\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"text\"}}],\"type\":\"TEXT_CONTENT\"}}}}",
            "{\"type\":\"citation-end\",\"index\":0}");

        var parts = await Stream(Model(handler, "command-a-03-2025", Ids("test-citation-id")), TestPrompt());

        Assert.Equal("Document", Assert.IsType<DocumentSourceStreamPart>(Assert.Single(parts, part => part is DocumentSourceStreamPart)).Title);
    }
}
