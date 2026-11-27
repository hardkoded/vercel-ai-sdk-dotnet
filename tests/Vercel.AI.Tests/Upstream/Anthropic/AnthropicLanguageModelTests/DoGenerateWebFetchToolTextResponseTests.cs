// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web fetch tool &gt; text response</c>.</summary>
public sealed class DoGenerateWebFetchToolTextResponseTests
{
    private static async Task<(CaptureHandler Handler, LanguageModelGenerateResult Result)> Run()
    {
        var handler = JsonFixture("anthropic-web-fetch-tool.1");
        var result = await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool("anthropic.web_fetch_20250910", "web_fetch", """{"maxUses":1}"""),
                ]
            )
        );

        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > text response::should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var (handler, _) = await Run();

        AssertJson(
            HelloBody(
                """{"tools":[{"max_uses":1,"name":"web_fetch","type":"web_fetch_20250910"}]}"""
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > text response::should include web fetch tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_web_fetch_tool_call_and_result_in_content()
    {
        var (_, result) = await Run();

        AssertJson(
            Snapshot(
                "doGenerate > web fetch tool > text response > should include web fetch tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
