// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>built-in google_search tool</c>.
/// </summary>
public sealed class DoGenerateBuiltInGoogleSearchToolTests
{
    private const string Prefix = DoGenerateSupport.Id + "built-in google_search tool::";

    private static LanguageModelCallOptions Options()
    {
        var options = GoogleUpstream.Prompt(new UserModelMessage("Hello, how are you?"));
        options.Tools =
        [
            LanguageModelTool.ProviderDefined(
                "google.google_search",
                "google_search",
                GoogleUpstream.Element("{}")
            ),
        ];
        return options;
    }

    private static SequencedHandler Handler()
    {
        return new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("google-search")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits a google_search tool descriptor on the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Emits_a_google_search_tool_descriptor_on_the_request_body()
    {
        var handler = Handler();

        await GoogleInteractionsStreamSupport
            .Model(handler)
            .DoGenerateAsync(Options(), CancellationToken.None);

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler)["tools"],
            "[{\"type\":\"google_search\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "parses google_search_call/result blocks as provider-executed tool parts and surfaces sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Parses_google_search_call_result_blocks_as_provider_executed_tool_parts_and_surfaces_sources()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Handler())
            .DoGenerateAsync(Options(), CancellationToken.None);

        var toolCall = result.Content.OfType<GeneratedToolCall>().First();
        Assert.Equal("tool-call", toolCall.Type);
        Assert.Equal("google_search", toolCall.ToolName);
        Assert.True(toolCall.ProviderExecuted);

        var toolResult = result.Content.OfType<GeneratedToolResult>().First();
        Assert.Equal("tool-result", toolResult.Type);
        Assert.Equal("google_search", toolResult.ToolName);

        var urls = result.Content.OfType<GeneratedSource>().Select(source => source.Url).ToList();
        Assert.True(urls.Count > 0);
        foreach (var url in urls)
        {
            Assert.Matches("^https?://", url);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "finishes with reason \"stop\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Finishes_with_reason_stop()
    {
        var result = await GoogleInteractionsStreamSupport
            .Model(Handler())
            .DoGenerateAsync(Options(), CancellationToken.None);

        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }
}
