// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream &gt; web fetch 20260209 tool &gt; input provided in content_block_start</c>.</summary>
public sealed class DoStreamWebFetch20260209ToolInputProvidedInContentBlockStartTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream > web fetch 20260209 tool > input provided in content_block_start::";

    private static Task<List<LanguageModelStreamPart>> StreamAsync()
    {
        var options = AnthropicParity.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "anthropic.web_fetch_20260209",
                "web_fetch",
                AnthropicParity.Json("""{}""")
            ),
            LanguageModelTool.ProviderDefined(
                "anthropic.code_execution_20260120",
                "code_execution",
                AnthropicParity.Json("""{}""")
            ),
        };
        return DoStreamSupport.Collect(
            DoStreamSupport.Provider(
                DoStreamSupport.FixtureStream("anthropic-web-fetch-tool-20260209.1")
            ),
            "claude-3-haiku-20240307",
            options
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include input from content_block_start in web_fetch tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_input_from_content_block_start_in_web_fetch_tool_call()
    {
        var streamArray = await StreamAsync();

        var toolCallEnd = streamArray
            .OfType<ToolCallStreamPart>()
            .FirstOrDefault(part => part.ToolName == "web_fetch");
        Assert.NotNull(toolCallEnd);
        Assert.Equal("tool-call", toolCallEnd!.Type);
        Assert.Equal("web_fetch", toolCallEnd.ToolName);
        Assert.Equal("{\"url\":\"https://example.com\"}", toolCallEnd.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream web fetch 20260209 tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_web_fetch_20260209_tool_results()
    {
        var parts = await StreamAsync();

        DoStreamSupport.AssertStreamSnapshot(parts, "web-fetch-20260209-stream");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve dynamic filtering callers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_dynamic_filtering_callers()
    {
        var streamArray = await StreamAsync();
        var codeExecutionCall = streamArray
            .OfType<ToolCallStreamPart>()
            .FirstOrDefault(part => part.ToolName == "code_execution");
        var webFetchCall = streamArray
            .OfType<ToolCallStreamPart>()
            .FirstOrDefault(part => part.ToolName == "web_fetch");
        var webFetchResult = streamArray
            .OfType<ToolResultStreamPart>()
            .FirstOrDefault(part => part.ToolName == "web_fetch");

        JsonAssert.Equal(
            codeExecutionCall!
                .ProviderMetadata!.Value.GetProperty("anthropic")
                .GetProperty("caller"),
            """{"type":"direct"}"""
        );
        JsonAssert.Equal(
            webFetchCall!.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("caller"),
            """{"type":"code_execution_20260120","toolId":"srvtoolu_01LKcA5qc1HwvLQSe3cLKmcK"}"""
        );
        JsonAssert.Equal(
            webFetchResult!.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("caller"),
            """{"type":"code_execution_20260120","toolId":"srvtoolu_01LKcA5qc1HwvLQSe3cLKmcK"}"""
        );
    }
}
