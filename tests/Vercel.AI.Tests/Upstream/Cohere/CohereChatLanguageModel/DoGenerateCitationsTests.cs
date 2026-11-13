// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>citations</c>.</summary>
public sealed class DoGenerateCitationsTests
{
    private const string UpstreamCommit = "3aad18a4178eada4ef31b4c169933a2dfe0f792f";
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::doGenerate > citations::";

    [Fact]
    [UpstreamTest(Chat + "should map document citations without title and with missing fields", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_map_document_citations_without_title_and_with_missing_fields()
    {
        var handler = JsonBody("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Automation of tasks\"}],\"citations\":[{\"text\":\"Automation of tasks\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"AI provides automation of tasks.\",\"extra_field\":\"passed through\"}}],\"type\":\"TEXT_CONTENT\"}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"tokens\":{\"input_tokens\":10,\"output_tokens\":5}}}");

        var result = await Model(handler, "command-r-plus", Ids("test-citation-id")).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            part => Assert.Equal("Automation of tasks", Assert.IsType<GeneratedText>(part).Text),
            part => AssertSource(
                part,
                "test-citation-id",
                "Document",
                "{\"cohere\":{\"text\":\"Automation of tasks\",\"sources\":[{\"type\":\"document\",\"id\":\"doc:0\",\"document\":{\"id\":\"doc:0\",\"text\":\"AI provides automation of tasks.\",\"extra_field\":\"passed through\"}}],\"citationType\":\"TEXT_CONTENT\"}}"));
    }

    [Fact]
    [UpstreamTest(Chat + "should not emit sources for tool-sourced citations", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public async Task Should_not_emit_sources_for_tool_sourced_citations()
    {
        var handler = JsonBody("{\"message\":{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"It is sunny in Berlin.\"}],\"citations\":[{\"start\":6,\"end\":11,\"text\":\"sunny\",\"sources\":[{\"type\":\"tool\",\"id\":\"getWeather:0\",\"tool_output\":{\"city\":\"Berlin\",\"condition\":\"sunny\"}}],\"type\":\"TEXT_CONTENT\"},{\"start\":0,\"end\":5,\"text\":\"It is\",\"sources\":null}]},\"finish_reason\":\"COMPLETE\",\"usage\":{\"tokens\":{\"input_tokens\":10,\"output_tokens\":5}}}");

        var result = await Model(handler).DoGenerateAsync(TestPrompt(), CancellationToken.None);

        Assert.Equal("It is sunny in Berlin.", Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text);
    }
}
