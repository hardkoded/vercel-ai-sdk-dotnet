// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; advisor tool &gt; with fixture</c>.</summary>
public sealed class DoGenerateAdvisorToolWithFixtureTests
{
    private static async Task<(CaptureHandler Handler, LanguageModelGenerateResult Result)> Run()
    {
        var handler = JsonFixture("anthropic-advisor-20260301.1");
        var result = await DoGenerate(
            handler,
            Options(
                tools:
                [
                    ProviderTool(
                        "anthropic.advisor_20260301",
                        "advisor",
                        """{"model":"claude-opus-4-7","maxTokens":2048}"""
                    ),
                ]
            ),
            "claude-sonnet-4-6"
        );

        return (handler, result);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > advisor tool > with fixture::should send the advisor tool in the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_advisor_tool_in_the_request_body()
    {
        var (handler, _) = await Run();

        AssertJson(
            HelloBody(
                """{"tools":[{"max_tokens":2048,"model":"claude-opus-4-7","name":"advisor","type":"advisor_20260301"}]}""",
                "claude-sonnet-4-6",
                128000
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > advisor tool > with fixture::should parse advisor calls and results as provider-executed tool parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_parse_advisor_calls_and_results_as_provider_executed_tool_parts()
    {
        var (_, result) = await Run();
        var content = ContentJson(result.Content).AsArray();

        var advisorToolCall = content.FirstOrDefault(part =>
            (string?)part!["type"] == "tool-call" && (string?)part["toolName"] == "advisor"
        );
        AssertJson(
            """{"input":"{}","providerExecuted":true,"toolCallId":"srvtoolu_01H51YYF9DTtdAHfcxxTs1NV","toolName":"advisor","type":"tool-call"}""",
            advisorToolCall
        );

        var advisorToolResult = content.FirstOrDefault(part =>
            (string?)part!["type"] == "tool-result" && (string?)part["toolName"] == "advisor"
        );
        Assert.NotNull(advisorToolResult);
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-result","toolCallId":"srvtoolu_01H51YYF9DTtdAHfcxxTs1NV","toolName":"advisor","result":{"type":"advisor_result"}}"""
            ),
            advisorToolResult
        );
        Assert.Contains(
            "Deliver design outline first",
            (string)advisorToolResult["result"]!["text"]!
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > advisor tool > with fixture::should expose advisor usage iterations in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_advisor_usage_iterations_in_provider_metadata()
    {
        var (_, result) = await Run();

        AssertJson(
            """
            [
              {"inputTokens":1051,"outputTokens":35,"type":"message"},
              {"inputTokens":2728,"model":"claude-opus-4-7","outputTokens":874,"type":"advisor_message"},
              {"inputTokens":1363,"outputTokens":3165,"type":"message"}
            ]
            """,
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["anthropic"]!["iterations"]
        );
    }
}
