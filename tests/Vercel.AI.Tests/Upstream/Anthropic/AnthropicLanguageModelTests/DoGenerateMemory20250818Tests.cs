// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; memory 20250818</c>.</summary>
public sealed class DoGenerateMemory20250818Tests
{
    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > memory 20250818::should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var handler = JsonFixture("anthropic-memory-20250818.1");

        await DoGenerate(
            handler,
            Options(tools: [ProviderTool("anthropic.memory_20250818", "memory")])
        );

        AssertJson(
            HelloBody("""{"tools":[{"name":"memory","type":"memory_20250818"}]}"""),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "context-management-2025-06-27"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > memory 20250818::should include memory tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_memory_tool_call_and_result_in_content()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-memory-20250818.1"),
            Options(tools: [ProviderTool("anthropic.memory_20250818", "memory")])
        );

        AssertJson(
            Snapshot(
                "doGenerate > memory 20250818 > should include memory tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
