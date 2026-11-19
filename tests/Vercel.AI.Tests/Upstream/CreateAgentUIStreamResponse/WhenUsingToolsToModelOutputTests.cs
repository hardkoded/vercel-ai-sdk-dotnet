// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AspNetCore;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.CreateAgentUIStream;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse.CreateAgentUIStreamResponseSupport;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStreamResponse;

/// <summary>Port of <c>create-agent-ui-stream-response.test.ts</c> &gt; <c>createAgentUIStreamResponse &gt; when using tools toModelOutput</c>.</summary>
public sealed class WhenUsingToolsToModelOutputTests : IAsyncLifetime
{
    private const string Prefix =
        "packages/ai/src/agent/create-agent-ui-stream-response.test.ts::createAgentUIStreamResponse > when using tools toModelOutput::";

    private readonly List<LanguageModelCallOptions> _recordedInputs = [];
    private IReadOnlyList<string> _decodedChunks = [];

    // beforeEach
    public async Task InitializeAsync()
    {
        var model = new Vercel.AI.Testing.TestLanguageModel
        {
            StreamParts =
            [
                new StreamStartStreamPart([]),
                new ResponseMetadataStreamPart("id-0", "mock-model-id", DateTimeOffset.UnixEpoch),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", "world!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, ModelUsage, "stop"),
            ],
        };
        var example = new Tool(
            "example",
            "Example tool",
            CreateAgentUIStreamSupport.Json(
                """{"type":"object","properties":{"input":{"type":"string"}},"required":["input"],"additionalProperties":false}"""
            ),
            null
        )
        {
            OutputSchema = CreateAgentUIStreamSupport.Json(
                """{"type":"object","properties":{"value":{"type":"string"}},"required":["value"]}"""
            ),
            // important: tool has toModelOutput that needs to be called
            ToModelOutput = call => new ContentToolModelOutput([
                new ToolModelContentPart(
                    "text",
                    JsonDocument
                        .Parse((string)call.Output!)
                        .RootElement.GetProperty("value")
                        .GetString()!
                ),
            ]),
        };
        var agent = new Agent(
            new AgentOptions { Model = model, Tools = [example] },
            CreateAgentUIStreamSupport.Client()
        );

        var response = await UIMessageStreamConverter.CreateAgentUIStreamResponse(
            new CreateAgentUIStreamResponseOptions
            {
                Agent = agent,
                UIMessages =
                [
                    new UIMessage(UIMessageRole.User, [new TextUIPart("Hello, world!")])
                    {
                        Id = "msg-1",
                    },
                    new UIMessage(
                        UIMessageRole.Assistant,
                        [
                            new ToolUIPart("example", "call-1", ToolUIPartState.OutputAvailable)
                            {
                                Input = CreateAgentUIStreamSupport.Json(
                                    """{"input":"Hello, world!"}"""
                                ),
                                Output = CreateAgentUIStreamSupport.Json(
                                    """{"value":"Example tool: Hello, world!"}"""
                                ),
                            },
                        ]
                    )
                    {
                        Id = "msg-2",
                    },
                ],
            }
        );

        _recordedInputs.AddRange(model.Calls);

        // consume the response
        _decodedChunks = await DecodeAsync(response);
        _recordedInputs.Clear();
        _recordedInputs.AddRange(model.Calls);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    [UpstreamTest(
        Prefix + "should have a single call that contains the tool result as text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_have_a_single_call_that_contains_the_tool_result_as_text()
    {
        var input = Assert.Single(_recordedInputs);
        Assert.Equal("ai-sdk-agent/tool-loop", input.Headers!["user-agent"]);
        Assert.False(input.IncludeRawChunks);
        Assert.Equal("auto", input.ToolChoice!.Type);

        Assert.Equal(3, input.Prompt.Count);
        var user = Assert.IsType<UserModelMessage>(input.Prompt[0]);
        Assert.Equal(
            "Hello, world!",
            Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text
        );

        var assistant = Assert.IsType<AssistantModelMessage>(input.Prompt[1]);
        var call = Assert.IsType<AssistantToolCallPart>(Assert.Single(assistant.Parts)).ToolCall;
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("example", call.ToolName);
        Assert.Equal(
            "Hello, world!",
            JsonDocument.Parse(call.ArgumentsJson).RootElement.GetProperty("input").GetString()
        );

        var tool = Assert.IsType<ToolModelMessage>(input.Prompt[2]);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("example", tool.ToolName);
        Assert.Equal("json", tool.OutputType);
        var text = Assert.IsType<TextContentPart>(Assert.Single(tool.OutputContent!));
        Assert.Equal("Example tool: Hello, world!", text.Text);

        var declared = Assert.Single(input.Tools!);
        Assert.Equal("example", declared.Name);
        Assert.Equal("Example tool", declared.Description);
        Assert.Equal(
            """{"type":"object","properties":{"input":{"type":"string"}},"required":["input"],"additionalProperties":false}""",
            declared.InputSchema.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the UI message stream response",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_the_UI_message_stream_response()
    {
        Assert.Equal(
            [
                "data: {\"type\":\"start\",\"messageId\":\"msg-2\"}\n\n",
                "data: {\"type\":\"start-step\"}\n\n",
                "data: {\"type\":\"text-start\",\"id\":\"1\"}\n\n",
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"Hello\"}\n\n",
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\", \"}\n\n",
                "data: {\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"world!\"}\n\n",
                "data: {\"type\":\"text-end\",\"id\":\"1\"}\n\n",
                "data: {\"type\":\"finish-step\"}\n\n",
                "data: {\"type\":\"finish\",\"finishReason\":\"stop\"}\n\n",
                "data: [DONE]\n\n",
            ],
            _decodedChunks
        );
    }
}
