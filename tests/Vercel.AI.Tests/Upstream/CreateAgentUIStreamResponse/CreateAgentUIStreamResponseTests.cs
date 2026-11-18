// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.CreateAgentUIStream;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.ToolLoopAgent;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse.CreateAgentUIStreamResponseSupport;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse;

/// <summary>Port of <c>create-agent-ui-stream-response.test.ts</c> &gt; <c>createAgentUIStreamResponse</c>.</summary>
public sealed class CreateAgentUIStreamResponseTests
{
    private const string Prefix =
        "packages/ai/src/agent/create-agent-ui-stream-response.test.ts::createAgentUIStreamResponse::";

    [Fact]
    [UpstreamTest(
        Prefix + "should reject repeated malformed parts before running the agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = "a3c6e3fef0790426005a11b4a0975cd48534f112"
    )]
    public async Task Should_reject_repeated_malformed_parts_before_running_the_agent()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model },
            CreateAgentUIStreamSupport.Client()
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            UIMessageStreamConverter.CreateAgentUIStreamResponse(
                new CreateAgentUIStreamResponseOptions
                {
                    Agent = agent,
                    UIMessagesJson = CreateAgentUIStreamSupport.Json(
                        """[{"id":"m1","role":"user","parts":["""
                            + string.Join(",", Enumerable.Repeat("{}", 2500))
                            + "]}]"
                    ),
                }
            )
        );

        Assert.IsType<TypeValidationException>(error);
        Assert.True(error.Message.Length < 40_000);
        var cause = Assert.IsType<UIMessagesSchemaException>(error.InnerException);
        Assert.Single(cause.Issues);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should run the agent with nested provider metadata without copying it",
        Coverage = UpstreamCoverage.Partial,
        Commit = "718824cd5959a15afe6d29765799659078e302c6",
        Note = "JsonElement is a value type, so the test compares the nested value instead of asserting reference identity."
    )]
    public async Task Should_run_the_agent_with_nested_provider_metadata()
    {
        var nested = "[" + string.Join(",", Enumerable.Repeat(NestedArrays(100), 20)) + "]";
        var model = new TestLanguageModel
        {
            StreamParts =
            [
                new StreamStartStreamPart([]),
                new FinishStreamPart(FinishReason.Stop, ModelUsage, "stop"),
            ],
        };

        var response = await UIMessageStreamConverter.CreateAgentUIStreamResponse(
            new CreateAgentUIStreamResponseOptions
            {
                Agent = new Agent(
                    new AgentOptions { Model = model },
                    CreateAgentUIStreamSupport.Client()
                ),
                UIMessagesJson = Json(
                    """[{"id":"m1","role":"assistant","parts":[{"type":"tool-x","toolCallId":"call-1","state":"output-denied","input":{},"approval":{"id":"approval-1","approved":false},"callProviderMetadata":{"p":{"value":"""
                        + nested
                        + "}}}]}]"
                ),
            }
        );
        await DecodeAsync(response);

        var call = Assert.Single(model.Calls);
        var toolCall = call
            .Prompt.OfType<AssistantModelMessage>()
            .SelectMany(message => message.Parts)
            .OfType<AssistantToolCallPart>()
            .Single();
        var value = toolCall.ProviderOptions!["p"].GetProperty("value");
        Assert.Equal(nested, value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_sandbox_to_tool_execution()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? receivedSandbox = null;
        var model = new SteeringTestModel(
            stream:
            [
                _ =>
                    SteeringTestModel.ToAsync([
                        new StreamStartStreamPart([]),
                        new ResponseMetadataStreamPart(
                            "id-0",
                            "mock-model-id",
                            DateTimeOffset.UnixEpoch
                        ),
                        new ToolCallStreamPart("call-1", "testTool", "{ \"value\": \"test\" }"),
                        new FinishStreamPart(FinishReason.ToolCalls, ModelUsage, "tool-calls"),
                    ]),
                _ => SteeringTestModel.ToAsync(TextParts("id-1", "Done")),
            ]
        );
        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Tools =
                [
                    ToolLoopAgentToolCalls.TestTool(
                        (value, options) =>
                        {
                            receivedSandbox = options.ExperimentalSandbox;
                            return value + "-result";
                        }
                    ),
                ],
            },
            CreateAgentUIStreamSupport.Client()
        );

        var response = await UIMessageStreamConverter.CreateAgentUIStreamResponse(
            new CreateAgentUIStreamResponseOptions
            {
                Agent = agent,
                UIMessages =
                [
                    new UIMessage(UIMessageRole.User, [new TextUIPart("Run the test tool")])
                    {
                        Id = "msg-1",
                    },
                ],
                ExperimentalSandbox = sandbox,
            }
        );

        await response.Body.ToArrayAsync();

        Assert.Same(sandbox, receivedSandbox);
    }

    private static string NestedArrays(int depth) =>
        new string('[', depth) + "[]" + new string(']', depth);

    private static JsonElement Json(string json) =>
        JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4096 }).RootElement.Clone();
}
