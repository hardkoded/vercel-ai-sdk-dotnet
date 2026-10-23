// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doStream</c> &gt; <c>built-in google_search tool</c>.
/// </summary>
public sealed class DoStreamBuiltInGoogleSearchToolTests
{
    private const string Prefix = "packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel.doStream > built-in google_search tool::";

    private static async Task<List<LanguageModelStreamPart>> Parts()
    {
        var model = GoogleInteractionsStreamSupport.Model(GoogleInteractionsStreamSupport.FixtureHandler("google-search"));
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        // Upstream also passes the google_search tool. The port does not send Interactions tools yet, and the stream does not depend on them.
        options.IncludeRawChunks = false;
        return await GoogleInteractionsStreamSupport.Stream(model, options);
    }

    [Fact]
    [UpstreamTest(Prefix + "emits provider-executed tool-call/tool-result and source parts for google_search results", Coverage = UpstreamCoverage.Covered)]
    public async Task Emits_provider_executed_tool_call_tool_result_and_source_parts_for_google_search_results()
    {
        var parts = await Parts();

        var toolCall = Assert.IsType<ToolCallStreamPart>(parts.First(part => part is ToolCallStreamPart));
        Assert.Equal("google_search", toolCall.ToolName);
        Assert.True(toolCall.ProviderExecuted);

        var toolResult = Assert.IsType<ToolResultStreamPart>(parts.First(part => part is ToolResultStreamPart));
        Assert.Equal("google_search", toolResult.ToolName);

        var sourceUrls = parts.OfType<SourceStreamPart>().Select(source => source.Url).ToList();
        Assert.True(sourceUrls.Count > 0);
        foreach (var url in sourceUrls)
        {
            Assert.Matches("^https?://", url);
        }

        var finish = Assert.IsType<FinishStreamPart>(parts.First(part => part is FinishStreamPart));
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "de-duplicates source urls when the same url appears in tool-result and text_annotation", Coverage = UpstreamCoverage.Covered)]
    public async Task De_duplicates_source_urls_when_the_same_url_appears_in_tool_result_and_text_annotation()
    {
        var parts = await Parts();

        var sourceUrls = parts.OfType<SourceStreamPart>().Select(source => source.Url).ToList();

        // Each unique URL appears at most once, however many times it surfaced across tool-result and text_annotation events.
        foreach (var group in sourceUrls.GroupBy(url => url))
        {
            Assert.Single(group);
        }
    }
}
