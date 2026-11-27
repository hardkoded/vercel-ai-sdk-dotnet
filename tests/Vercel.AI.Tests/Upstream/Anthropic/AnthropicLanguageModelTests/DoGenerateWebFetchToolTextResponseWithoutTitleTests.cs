// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web fetch tool &gt; text response without title</c>.</summary>
public sealed class DoGenerateWebFetchToolTextResponseWithoutTitleTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > text response without title::should include web fetch tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_web_fetch_tool_call_and_result_in_content()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-web-fetch-tool.2"),
            Options(
                tools:
                [
                    ProviderTool("anthropic.web_fetch_20250910", "web_fetch", """{"maxUses":1}"""),
                ]
            )
        );

        AssertJson(
            Snapshot(
                "doGenerate > web fetch tool > text response without title > should include web fetch tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
