// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; custom tool</c>.</summary>
public sealed class DoGenerateCustomToolTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > custom tool::";

    private static async Task<(OpenAICapture Capture, LanguageModelGenerateResult Result)> Run()
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
        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5.2-codex").DoGenerateAsync(options, CancellationToken.None);
        return (capture, result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send request body with custom tool", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body_with_custom_tool()
    {
        var (capture, _) = await Run();

        JsonAssert.Equal(
            JsonNode.Parse(capture.Body),
            """{"model":"gpt-5.2-codex","input":[{"role":"user","content":[{"type":"input_text","text":"Hello"}]}],"tools":[{"type":"custom","name":"write_sql","description":"Write a SQL SELECT query to answer the user question.","format":{"type":"grammar","syntax":"regex","definition":"SELECT .+"}}]}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should generate custom tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_generate_custom_tool_call()
    {
        var (_, result) = await Run();

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_custom_sql_001", call.ToolCallId);
        Assert.Equal("write_sql", call.ToolName);
        Assert.Equal("\"SELECT * FROM users WHERE age > 25\"", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, """{"openai":{"itemId":"ct_abc123def456"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should have tool-calls finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_have_tool_calls_finish_reason()
    {
        var (_, result) = await Run();

        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
    }
}
