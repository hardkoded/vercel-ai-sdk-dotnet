// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; code execution 20250825</c>.</summary>
public sealed class DoGenerateCodeExecution20250825Tests
{
    private static LanguageModelCallOptions Call()
    {
        return Options(
            tools: [ProviderTool("anthropic.code_execution_20250825", "code_execution")]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250825::should send request body with include and tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_include_and_tool()
    {
        var handler = JsonFixture("anthropic-code-execution-20250825.1");

        await DoGenerate(handler, Call());

        AssertJson(
            HelloBody("""{"tools":[{"name":"code_execution","type":"code_execution_20250825"}]}"""),
            RequestBody(handler)
        );
        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "code-execution-2025-08-25"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250825::should include code execution tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_code_execution_tool_call_and_result_in_content()
    {
        var result = await DoGenerate(JsonFixture("anthropic-code-execution-20250825.1"), Call());

        AssertJson(
            Snapshot(
                "doGenerate > code execution 20250825 > should include code execution tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250825::should expose container information as provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_container_information_as_provider_metadata()
    {
        var result = await DoGenerate(JsonFixture("anthropic-code-execution-20250825.1"), Call());

        AssertJson(
            Snapshot(
                "doGenerate > code execution 20250825 > should expose container information as provider metadata"
            ),
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > code execution 20250825::should include file id list in code execution tool generate call result.",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_file_id_list_in_code_execution_tool_generate_call_result()
    {
        var handler = JsonFixture("anthropic-code-execution-20250825.2");

        var result = await DoGenerate(handler, Call());

        var expected = Snapshot(
            "doGenerate > code execution 20250825 > should include file id list in code execution tool generate call result."
        );
        AssertJson(expected["content"], ContentJson(result.Content));
        AssertJson(
            expected["finishReason"],
            FinishReasonJson(result.FinishReason, result.RawFinishReason)
        );
        AssertJson(
            expected["providerMetadata"],
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
        );
        AssertJson(expected["request"]!["body"], JsonNode.Parse(handler.Body));
        AssertJson(expected["response"]!["body"], JsonNode.Parse(result.RawResponse!));
        Assert.Equal((string?)expected["response"]!["id"], result.ResponseId);
        Assert.Equal((string?)expected["response"]!["modelId"], result.ResponseModelId);
        Assert.StartsWith(
            (string)expected["response"]!["headers"]!["content-type"]!,
            result.ResponseHeaders["content-type"]
        );
        Assert.Equal(
            (string)expected["response"]!["headers"]!["content-length"]!,
            result.ResponseHeaders["content-length"]
        );
        AssertJson(expected["usage"], UsageJson(result.Usage));
        AssertJson(expected["warnings"], WarningsJson(result.Warnings));
    }
}
