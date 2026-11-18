// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.CreateAgentUIStream;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse.CreateAgentUIStreamResponseSupport;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse;

/// <summary>Port of <c>create-agent-ui-stream-response.test.ts</c> &gt; <c>createAgentUIStreamResponse &gt; when using onFinish without originalMessages</c>.</summary>
public sealed class WhenUsingOnFinishWithoutOriginalMessagesTests
{
    private const string Prefix =
        "packages/ai/src/agent/create-agent-ui-stream-response.test.ts::createAgentUIStreamResponse > when using onFinish without originalMessages::";

    [Fact]
    [UpstreamTest(
        Prefix + "should call onFinish callback and complete stream without errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onFinish_callback_and_complete_stream_without_errors()
    {
        var onFinishCalled = false;
        IReadOnlyList<UIMessage>? finishMessages = null;
        var uiStepEnds = new List<UIMessageStreamOnStepEndEvent>();

        var agent = new Agent(
            new AgentOptions
            {
                Model = new TestLanguageModel
                {
                    StreamParts =
                    [
                        new StreamStartStreamPart([]),
                        new ResponseMetadataStreamPart(
                            "id-0",
                            "mock-model-id",
                            DateTimeOffset.UnixEpoch
                        ),
                        new TextStartStreamPart("1"),
                        new TextDeltaStreamPart("1", "Done!"),
                        new TextEndStreamPart("1"),
                        new FinishStreamPart(FinishReason.Stop, ModelUsage, "stop"),
                    ],
                },
                Tools =
                [
                    new Tool(
                        "testTool",
                        "Test tool",
                        CreateAgentUIStreamSupport.Json(
                            """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"],"additionalProperties":false}"""
                        ),
                        null
                    )
                    {
                        OutputSchema = CreateAgentUIStreamSupport.Json(
                            """{"type":"object","properties":{"result":{"type":"string"}},"required":["result"]}"""
                        ),
                    },
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
                    new UIMessage(UIMessageRole.User, [new TextUIPart("Run test tool")])
                    {
                        Id = "msg-1",
                    },
                    new UIMessage(
                        UIMessageRole.Assistant,
                        [
                            new ToolUIPart("testTool", "call-1", ToolUIPartState.OutputAvailable)
                            {
                                Input = CreateAgentUIStreamSupport.Json("""{"value":"test"}"""),
                                Output = CreateAgentUIStreamSupport.Json(
                                    """{"result":"success"}"""
                                ),
                            },
                        ]
                    )
                    {
                        Id = "msg-2",
                    },
                ],
                // Note: originalMessages is NOT provided, relying on auto-population
                UIMessageStreamOptions = new UIMessageStreamOptions
                {
#pragma warning disable CS0618
                    OnFinish = e =>
                    {
                        onFinishCalled = true;
                        finishMessages = e.Messages;
                        return Task.CompletedTask;
                    },
#pragma warning restore CS0618
                },
                OnUIMessageStepEnd = e =>
                {
                    uiStepEnds.Add(e);
                    return Task.CompletedTask;
                },
            }
        );

        // Consume the response to trigger onFinish
        var decodedChunks = await DecodeAsync(response);

        // Verify stream completed successfully
        Assert.Contains(decodedChunks, chunk => chunk.Contains("\"type\":\"finish\""));

        // Verify onFinish was called with messages
        Assert.True(onFinishCalled);
        Assert.NotNull(finishMessages);
        Assert.Equal(2, finishMessages!.Count);
        var stepEnd = Assert.Single(uiStepEnds);
        Assert.Equal("msg-2", stepEnd.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, stepEnd.ResponseMessage.Role);
        Assert.Contains(stepEnd.ResponseMessage.Parts, part =>
            part is TextUIPart { Text: "Done!" }
        );
    }
}
