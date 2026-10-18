// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; custom tool</c>.</summary>
public sealed class DoGenerateCustomToolTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > custom tool::";

    [Fact]
    [UpstreamTest(Id + "should send request body with custom tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_request_body_with_custom_tool()
    {
        using var args = JsonDocument.Parse(
            """{"description":"Write a SQL SELECT query to answer the user question.","format":{"type":"grammar","syntax":"regex","definition":"SELECT .+"}}""");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = new[] { LanguageModelTool.ProviderDefined("openai.custom", "write_sql", args.RootElement.Clone()) },
        };

        var request = OpenAIResponsesLanguageModel.Prepare("gpt-5.2-codex", options, false);

        var expected = JsonNode.Parse(
            """
            {
              "input": [{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],
              "model": "gpt-5.2-codex",
              "tools": [{
                "type": "custom",
                "name": "write_sql",
                "description": "Write a SQL SELECT query to answer the user question.",
                "format": {"type":"grammar","syntax":"regex","definition":"SELECT .+"}
              }]
            }
            """);
        Assert.True(JsonNode.DeepEquals(expected, request.Body), request.Body.ToJsonString());
    }

    private static async Task<LanguageModelGenerateResult> Generate()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIResponsesSupport.Fixture("openai-custom-tool.1.json") };
        var options = OpenAIUpstream.Hello();
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(
                "openai.custom",
                "write_sql",
                OpenAIUpstream.Json("""{"description":"Write a SQL SELECT query to answer the user question.","format":{"type":"grammar","syntax":"regex","definition":"SELECT .+"}}""")),
        };
        return await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.2-codex").DoGenerateAsync(options, CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Id + "should generate custom tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_custom_tool_call()
    {
        var result = await Generate();

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_custom_sql_001", call.ToolCallId);
        Assert.Equal("write_sql", call.ToolName);
        Assert.Equal("\"SELECT * FROM users WHERE age > 25\"", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, """{"openai":{"itemId":"ct_abc123def456"}}""");
    }

    [Fact]
    [UpstreamTest(Id + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        var result = await Generate();

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }
}
