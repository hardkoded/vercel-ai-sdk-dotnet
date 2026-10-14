// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doGenerate &gt; x_search tool</c>.</summary>
public sealed class DoGenerateXSearchToolTests
{
    private const string Prefix = TestFile + " > doGenerate > x_search tool::";

    [Fact]
    [UpstreamTest(Prefix + "should include x_search tool call with real response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_x_search_tool_call_with_real_response()
    {
        var result = await Model(JsonFixture("xai-x-search-tool.1")).DoGenerateAsync(WithTools(ProviderTool("xai.x_search", "x_search")), CancellationToken.None);

        JsonAssert.Equal(ToJson(result.Content), Expected("xai-x-search-tool.1.content.json").ToJsonString());
    }
}
