// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; mcp tool approval</c>.</summary>
public sealed class DoGenerateMcpToolApprovalTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > mcp tool approval::";

    private static LanguageModelTool McpTool() =>
        LanguageModelTool.ProviderDefined(
            "openai.mcp",
            "MCP",
            OpenAIUpstream.Json(
                """{"serverLabel":"zip1","serverUrl":"https://zip1.io/mcp","serverDescription":"Link shortener","requireApproval":"always"}"""
            )
        );

    private static OpenAIResponsesLanguageModel Model(OpenAICapture capture)
    {
        var model = OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-mini");
        var next = 0;
        model.GenerateId = () => "id-" + next++;
        return model;
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should emit tool-call and tool-approval-request when model requests approval (turn 1)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_tool_call_and_tool_approval_request_when_model_requests_approval_turn_1()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.1.json"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Tools = new[] { McpTool() },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        var expected = JsonNode.Parse(
            """{"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user"}],"model":"gpt-5-mini","tools":[{"require_approval":"always","server_description":"Link shortener","server_label":"zip1","server_url":"https://zip1.io/mcp","type":"mcp"}]}"""
        );
        var body = JsonNode.Parse(capture.Body)!;
        Assert.True(JsonNode.DeepEquals(expected, body), body.ToJsonString());

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-mcp-tool-approval.1");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit text response after denial (turn 2)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_text_response_after_denial_turn_2()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.2.json"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall(
                            "mcpr_04f6b17429cf2b02006949a6712b1081968b3c7a72dec695d8",
                            "mcp.create_short_url",
                            """{"url":"https://ai-sdk.dev/"}"""
                        )
                        {
                            ProviderExecuted = true,
                        },
                    },
                    null
                ),
                new ToolApprovalResponseModelMessage(
                    "mcpr_04f6b17429cf2b02006949a6712b1081968b3c7a72dec695d8",
                    false
                ),
            },
            Tools = new[] { McpTool() },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-mcp-tool-approval.2");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit new tool-approval-request when retrying (turn 3)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_new_tool_approval_request_when_retrying_turn_3()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.3.json"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall(
                            "mcpr_04f6b17429cf2b02006949a6712b1081968b3c7a72dec695d8",
                            "mcp.create_short_url",
                            """{"url":"https://ai-sdk.dev/"}"""
                        )
                        {
                            ProviderExecuted = true,
                        },
                    },
                    null
                ),
                new ToolApprovalResponseModelMessage(
                    "mcpr_04f6b17429cf2b02006949a6712b1081968b3c7a72dec695d8",
                    false
                ),
                new AssistantModelMessage("The tool was not approved.", null, null),
                new UserModelMessage("try again"),
            },
            Tools = new[] { McpTool() },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-mcp-tool-approval.3");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit tool-call with result after approval (turn 4)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_emit_tool_call_with_result_after_approval_turn_4()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.4.json"),
        };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall(
                            "mcpr_04f6b17429cf2b02006949a68bf5808196b6f2008a315c9aa4",
                            "mcp.create_short_url",
                            """{"url":"https://ai-sdk.dev/"}"""
                        )
                        {
                            ProviderExecuted = true,
                        },
                    },
                    null
                ),
                new ToolApprovalResponseModelMessage(
                    "mcpr_04f6b17429cf2b02006949a68bf5808196b6f2008a315c9aa4",
                    true
                ),
            },
            Tools = new[] { McpTool() },
        };

        var result = await Model(capture).DoGenerateAsync(options, CancellationToken.None);

        OpenAIResponsesSupport.AssertContentSnapshot(result.Content, "openai-mcp-tool-approval.4");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }
}
