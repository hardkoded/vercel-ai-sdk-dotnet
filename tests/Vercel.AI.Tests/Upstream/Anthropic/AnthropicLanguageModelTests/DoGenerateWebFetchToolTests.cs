// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web fetch tool</c>.</summary>
public sealed class DoGenerateWebFetchToolTests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool::should use web_fetch_20260209 for anthropic.tools.webFetch_20260209",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_web_fetch_20260209_for_anthropic_tools_webFetch_20260209()
    {
        var handler = JsonValue(
            """
            {"type":"message","id":"msg_test","content":[{"type":"text","text":"Fetched result."}],"stop_reason":"end_turn","usage":{"input_tokens":10,"output_tokens":20}}
            """
        );

        await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool("anthropic.web_fetch_20260209", "web_fetch", """{"maxUses":1}"""),
                ]
            )
        );

        var tools = RequestBody(handler)["tools"]!.AsArray();
        Assert.Single(tools);
        AssertJson("""{"type":"web_fetch_20260209","name":"web_fetch","max_uses":1}""", tools[0]);
        Assert.Equal("code-execution-web-tools-2026-02-09", Beta(handler));
    }
}
