// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; mcp tool</c>.</summary>
public sealed class DoGenerateMcpToolTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > mcp tool::";

    [Fact]
    [UpstreamTest(Prefix + "should send request body with tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body_with_tool()
    {
        var (_, capture) = await Run();

        var expected = JsonNode.Parse("""{"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user"}],"model":"gpt-5-mini","tools":[{"require_approval":"never","server_description":"A web-search API for AI agents","server_label":"dmcp","server_url":"https://mcp.exa.ai/mcp","type":"mcp"}]}""");
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body), body.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should include mcp tool call and result in content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_mcp_tool_call_and_result_in_content()
    {
        var (result, _) = await Run();

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-mcp-tool.1");
    }

    private static async Task<(LanguageModelGenerateResult Result, OpenAICapture Capture)> Run()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture("openai-mcp-tool.1.json") };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "openai.mcp",
                "MCP",
                OpenAIUpstream.Json("""{"serverLabel":"dmcp","serverUrl":"https://mcp.exa.ai/mcp","serverDescription":"A web-search API for AI agents"}""")),
        };

        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-mini").DoGenerateAsync(options, CancellationToken.None);
        return (result, capture);
    }
}
