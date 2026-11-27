// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web fetch tool &gt; 20260209 text response</c>.</summary>
public sealed class DoGenerateWebFetchTool20260209TextResponseTests
{
    private static async Task<LanguageModelGenerateResult> Run()
    {
        return await DoGenerate(
            JsonFixture("anthropic-web-fetch-tool-20260209.1"),
            Options(
                tools:
                [
                    ProviderTool("anthropic.web_fetch_20260209", "web_fetch"),
                    ProviderTool("anthropic.code_execution_20260120", "code_execution"),
                ]
            )
        );
    }

    private static JsonNode? Find(JsonNode content, string type, string toolName)
    {
        return content
            .AsArray()
            .FirstOrDefault(part =>
                (string?)part!["type"] == type && (string?)part["toolName"] == toolName
            );
    }

    // toEqual treats a property that is undefined like a missing one.
    private static JsonNode? WithoutNulls(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, value) in obj.ToList())
            {
                if (value == null)
                {
                    obj.Remove(key);
                }
            }
        }

        return node;
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > 20260209 text response::should include web fetch tool call with input in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_web_fetch_tool_call_with_input_in_content()
    {
        var result = await Run();

        var webFetchToolCall = Find(ContentJson(result.Content), "tool-call", "web_fetch");
        Assert.NotNull(webFetchToolCall);
        AssertJsonContains(
            JsonNode.Parse(
                """{"type":"tool-call","toolName":"web_fetch","input":"{\"url\":\"https://example.com\"}","providerExecuted":true}"""
            ),
            webFetchToolCall
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > 20260209 text response::should include web fetch 20260209 tool call and result in content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_web_fetch_20260209_tool_call_and_result_in_content()
    {
        var result = await Run();

        AssertJson(
            Snapshot(
                "doGenerate > web fetch tool > 20260209 text response > should include web fetch 20260209 tool call and result in content"
            ),
            ContentJson(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > web fetch tool > 20260209 text response::should preserve dynamic filtering callers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_dynamic_filtering_callers()
    {
        var result = await Run();
        var content = ContentJson(result.Content);

        var codeExecutionCall = Find(content, "tool-call", "code_execution");
        var webFetchCall = Find(content, "tool-call", "web_fetch");
        var webFetchResult = Find(content, "tool-result", "web_fetch");

        AssertJson(
            """{"type":"direct"}""",
            WithoutNulls(codeExecutionCall?["providerMetadata"]?["anthropic"]?["caller"])
        );
        const string Caller =
            """{"type":"code_execution_20260120","toolId":"srvtoolu_015CSHH7X69AhdK9gNzotEeh"}""";
        AssertJson(
            Caller,
            WithoutNulls(webFetchCall?["providerMetadata"]?["anthropic"]?["caller"])
        );
        AssertJson(
            Caller,
            WithoutNulls(webFetchResult?["providerMetadata"]?["anthropic"]?["caller"])
        );
    }
}
