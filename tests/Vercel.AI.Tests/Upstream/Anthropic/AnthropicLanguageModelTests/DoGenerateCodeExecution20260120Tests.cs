// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; code execution 20260120</c>.</summary>
public sealed class DoGenerateCodeExecution20260120Tests
{
    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20260120::should send request body with tool and no beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_tool_and_no_beta_header()
    {
        var handler = JsonFixture("anthropic-code-execution-20250825.1");

        await DoGenerate(
            handler,
            Options(tools: [ProviderTool("anthropic.code_execution_20260120", "code_execution")])
        );

        AssertJson(
            HelloBody("""{"tools":[{"name":"code_execution","type":"code_execution_20260120"}]}"""),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20260120::should include code execution tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_execution_tool_call_and_result_in_content()
    {
        var result = await DoGenerate(
            JsonFixture("anthropic-code-execution-20250825.1"),
            Options(tools: [ProviderTool("anthropic.code_execution_20260120", "code_execution")])
        );

        AssertJson(
            Snapshot(
                "doGenerate > code execution 20260120 > should include code execution tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }
}
