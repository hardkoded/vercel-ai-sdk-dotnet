// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; mcp tool approval</c>.</summary>
public sealed class DoStreamMcpToolApprovalTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > mcp tool approval::";

    private static LanguageModelTool McpTool() => LanguageModelTool.ProviderDefined(
        "openai.mcp",
        "MCP",
        OpenAIUpstream.Json("""{"serverLabel":"zip1","serverUrl":"https://zip1.io/mcp","serverDescription":"Link shortener","requireApproval":"always"}"""));

    private static OpenAIResponsesLanguageModel Model(OpenAICapture capture)
    {
        var model = OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-mini");
        var next = 0;
        model.GenerateId = () => "id-" + next++;
        return model;
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream tool-call and tool-approval-request when model requests approval (turn 1)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_call_and_tool_approval_request_when_model_requests_approval_turn_1()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.1.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            Tools = new[] { McpTool() },
        };

        var parts = await OpenAIUpstream.Read(await Model(capture).DoStreamAsync(options, CancellationToken.None));

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-mcp-tool-approval.1");
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text response after denial (turn 2)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_response_after_denial_turn_2()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.2.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("mcpr_04a97b4fce127879006949a83ac9308195a7f7b69ea82e91fe", "mcp.create_short_url", """{"url":"https://ai-sdk.dev/"}""") { ProviderExecuted = true } }, null),
                new ToolApprovalResponseModelMessage("mcpr_04a97b4fce127879006949a83ac9308195a7f7b69ea82e91fe", false),
            },
            Tools = new[] { McpTool() },
        };

        var parts = await OpenAIUpstream.Read(await Model(capture).DoStreamAsync(options, CancellationToken.None));

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-mcp-tool-approval.2");
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream new tool-approval-request when retrying (turn 3)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_new_tool_approval_request_when_retrying_turn_3()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.3.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("mcpr_04a97b4fce127879006949a83ac9308195a7f7b69ea82e91fe", "mcp.create_short_url", """{"url":"https://ai-sdk.dev/"}""") { ProviderExecuted = true } }, null),
                new ToolApprovalResponseModelMessage("mcpr_04a97b4fce127879006949a83ac9308195a7f7b69ea82e91fe", false),
                new AssistantModelMessage("The tool was not approved.", null, null),
                new UserModelMessage("try again"),
            },
            Tools = new[] { McpTool() },
        };

        var parts = await OpenAIUpstream.Read(await Model(capture).DoStreamAsync(options, CancellationToken.None));

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-mcp-tool-approval.3");
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream tool-call with result after approval (turn 4)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_call_with_result_after_approval_turn_4()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-mcp-tool-approval.4.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);
        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(events) };
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("shorten ai-sdk.dev"),
                new AssistantModelMessage(null, new[] { new GeneratedToolCall("mcpr_04a97b4fce127879006949a8672ac081959f95aa8ceedb7cd9", "mcp.create_short_url", """{"url":"https://ai-sdk.dev/"}""") { ProviderExecuted = true } }, null),
                new ToolApprovalResponseModelMessage("mcpr_04a97b4fce127879006949a8672ac081959f95aa8ceedb7cd9", true),
            },
            Tools = new[] { McpTool() },
        };

        var parts = await OpenAIUpstream.Read(await Model(capture).DoStreamAsync(options, CancellationToken.None));

        OpenAIResponsesSupport.AssertStreamSnapshot(parts, "openai-mcp-tool-approval.4");
    }
}
