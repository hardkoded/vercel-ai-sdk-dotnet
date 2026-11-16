// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel.MistralConversationFixtures;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralConversationPrepareTools;

/// <summary>Port of <c>mistral-conversation-prepare-tools.test.ts</c> &gt; <c>prepareConversationTools</c>.</summary>
public sealed class PrepareConversationToolsTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-conversation-prepare-tools.test.ts::prepareConversationTools::";

    [Fact]
    [UpstreamTest(
        Prefix + "maps both search tools and warns about unsupported provider tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_both_search_tools_and_warns_about_unsupported_provider_tools()
    {
        var result = Vercel.AI.Mistral.MistralConversationPrepareTools.PrepareConversationTools(
            new[]
            {
                Search(),
                Search("mistral.web_search_premium", "news"),
                Search("mistral.unknown", "unknown"),
            }
        );

        AssertJson("[{\"type\":\"web_search\"},{\"type\":\"web_search_premium\"}]", result.Tools);
        Assert.Null(result.ToolChoice);
        var warning = Assert.Single(result.ToolWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("provider-defined tool mistral.unknown", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "forces a function tool when search is also available",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Forces_a_function_tool_when_search_is_also_available()
    {
        var result = Vercel.AI.Mistral.MistralConversationPrepareTools.PrepareConversationTools(
            new[]
            {
                Search(),
                new LanguageModelTool("weather", null, Node("{\"type\":\"object\"}")),
            },
            ToolChoice.Tool("weather")
        );

        AssertJson(
            "[{\"type\":\"function\",\"function\":{\"name\":\"weather\",\"parameters\":{\"type\":\"object\"}}}]",
            result.Tools
        );
        Assert.Equal("any", result.ToolChoice);
        Assert.Empty(result.ToolWarnings);
    }
}
