// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.AspNetCore;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.CreateAgentUIStream.CreateAgentUIStreamSupport;

namespace Vercel.AI.Tests.Upstream.CreateAgentUIStream;

/// <summary>Port of <c>create-agent-ui-stream.test.ts</c> &gt; <c>createAgentUIStream</c>.</summary>
public sealed class CreateAgentUIStreamTests
{
    private const string Prefix =
        "packages/ai/src/agent/create-agent-ui-stream.test.ts::createAgentUIStream::";

    private static async Task Drain(IAsyncEnumerable<System.Text.Json.Nodes.JsonObject> stream)
    {
        await foreach (var _ in stream) { }
    }

    private static UIMessage UserText(string id, string text) =>
        new(UIMessageRole.User, [new TextUIPart(text)]) { Id = id };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "preserves generation callbacks and UI continuations (continuation: %s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_generation_callbacks_and_UI_continuations(bool isContinuation)
    {
        var events = new List<string>();
        var onStepFinishCalls = 0;
        var uiCalls = new List<UIMessageStreamOnStepEndEvent>();
        var originalMessage = isContinuation
            ? new UIMessage(UIMessageRole.Assistant, [new TextUIPart("Previous")])
            {
                Id = "assistant-1",
            }
            : UserText("user-1", "Hello");
        var stream = await UIMessageStreamConverter.CreateAgentUIStreamAsync(
            new CreateAgentUIStreamOptions
            {
                Agent = new Agent(
                    new AgentOptions
                    {
                        Model = CreateMockModel(),
                        OnStepEnd = (_, _) =>
                        {
                            lock (events)
                            {
                                events.Add("agent");
                            }

                            return Task.CompletedTask;
                        },
                    },
                    Client()
                ),
                UIMessages = [originalMessage],
                OnStepEnd = (_, _) =>
                {
                    lock (events)
                    {
                        events.Add("generation");
                    }

                    return Task.CompletedTask;
                },
#pragma warning disable CS0618
                OnStepFinish = (_, _) =>
                {
                    onStepFinishCalls++;
                    return Task.CompletedTask;
                },
#pragma warning restore CS0618
                OnUIMessageStepEnd = e =>
                {
                    lock (events)
                    {
                        events.Add("ui");
                    }

                    uiCalls.Add(e);
                    return Task.CompletedTask;
                },
                UIMessageStreamOptions = new UIMessageStreamOptions
                {
                    GenerateMessageId = () => "assistant-new",
                    OnEnd = _ =>
                    {
                        lock (events)
                        {
                            events.Add("end");
                        }

                        return Task.CompletedTask;
                    },
                },
            }
        );
        await Drain(stream);

        // Generation and UI callbacks can run in either order as the chunk
        // flows through the stream, but all must run before the stream ends.
        Assert.Equal(["agent", "generation", "ui"], events.Take(events.Count - 1).Order());
        Assert.Equal("end", events[^1]);
        var call = Assert.Single(uiCalls);
        Assert.Equal(isContinuation, call.IsContinuation);
        Assert.Equal(isContinuation ? "assistant-1" : "assistant-new", call.ResponseMessage.Id);
        Assert.Equal(isContinuation ? 1 : 2, call.Messages.Count);
        var expectedParts = new List<string>();
        if (isContinuation)
        {
            expectedParts.Add("text:Previous");
        }

        expectedParts.Add("step-start");
        expectedParts.Add("text:response:Done");
        Assert.Equal(
            expectedParts,
            call.ResponseMessage.Parts.Select(part =>
                part switch
                {
                    TextUIPart text when text.State == "done" => "text:" + text.Text + ":Done",
                    TextUIPart text => "text:" + text.Text,
                    _ => part.Type,
                }
            )
        );
        Assert.Equal(0, onStepFinishCalls);
        Assert.Single(originalMessage.Parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "provides accumulated UI messages after every agent step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Provides_accumulated_UI_messages_after_every_agent_step()
    {
        var toolExecutionCount = 0;
        var usage = new LanguageModelUsage(3, 4, 7);
        var model = new SteeringTestModel(
            stream:
            [
                _ =>
                    SteeringTestModel.ToAsync([
                        new StreamStartStreamPart([]),
                        new ResponseMetadataStreamPart(
                            "response-1",
                            "mock-model-id",
                            DateTimeOffset.UnixEpoch
                        ),
                        new ToolCallStreamPart(
                            "call-1",
                            "weather",
                            "{\"location\":\"San Francisco\"}"
                        ),
                        new FinishStreamPart(FinishReason.ToolCalls, usage, "tool-calls"),
                    ]),
                _ =>
                    SteeringTestModel.ToAsync([
                        new StreamStartStreamPart([]),
                        new ResponseMetadataStreamPart(
                            "response-2",
                            "mock-model-id",
                            DateTimeOffset.UnixEpoch
                        ),
                        new ReasoningStartStreamPart("reasoning-1"),
                        new ReasoningDeltaStreamPart("reasoning-1", "The tool returned 72°F."),
                        new ReasoningEndStreamPart("reasoning-1"),
                        new TextStartStreamPart("text-1"),
                        new TextDeltaStreamPart("text-1", "It is 72°F in San Francisco."),
                        new TextEndStreamPart("text-1"),
                        new FinishStreamPart(
                            FinishReason.Stop,
                            new LanguageModelUsage(5, 8, 13),
                            "stop"
                        ),
                    ]),
            ]
        );
        var weather = new Tool(
            "weather",
            null,
            Json(
                """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"]}"""
            ),
            (input, _) =>
            {
                toolExecutionCount++;
                return Task.FromResult(
                    JsonSerializer.Serialize(
                        new
                        {
                            location = input.GetProperty("location").GetString(),
                            temperature = 72,
                        }
                    )
                );
            }
        );
        var onStepEndCalls = 0;
        StepResult? firstStepEvent = null;
        var uiCalls = new List<UIMessageStreamOnStepEndEvent>();
        var agent = new Agent(new AgentOptions { Model = model, Tools = [weather] }, Client());
        var uiMessages = new[] { UserText("user-1", "What is the weather in San Francisco?") };

        var stream = await UIMessageStreamConverter.CreateAgentUIStreamAsync(
            new CreateAgentUIStreamOptions
            {
                Agent = agent,
                UIMessages = uiMessages,
                OnStepEnd = (step, _) =>
                {
                    onStepEndCalls++;
                    firstStepEvent ??= step;
                    return Task.CompletedTask;
                },
                OnUIMessageStepEnd = e =>
                {
                    uiCalls.Add(e);
                    return Task.CompletedTask;
                },
                UIMessageStreamOptions = new UIMessageStreamOptions
                {
                    GenerateMessageId = () => "assistant-1",
                    MessageMetadata = part =>
                        Json(JsonSerializer.Serialize(new { partType = part.Type })),
                },
            }
        );

        await Drain(stream);

        Assert.Equal(2, onStepEndCalls);
        Assert.Null(firstStepEvent!.GetType().GetProperty("ResponseMessage"));
        Assert.Equal(1, toolExecutionCount);
        Assert.Equal(2, uiCalls.Count);

        var first = uiCalls[0];
        Assert.False(first.IsContinuation);
        Assert.Equal(2, first.Messages.Count);
        Assert.Equal("user-1", first.Messages[0].Id);
        Assert.Equal(UIMessageRole.User, first.Messages[0].Role);
        Assert.Equal(
            "What is the weather in San Francisco?",
            Assert.IsType<TextUIPart>(Assert.Single(first.Messages[0].Parts)).Text
        );
        Assert.Equal("assistant-1", first.ResponseMessage.Id);
        Assert.Equal(UIMessageRole.Assistant, first.ResponseMessage.Role);
        Assert.Equal(
            "finish-step",
            first.ResponseMessage.Metadata!.Value.GetProperty("partType").GetString()
        );
        Assert.Equal(2, first.ResponseMessage.Parts.Count);
        Assert.IsType<StepStartUIPart>(first.ResponseMessage.Parts[0]);
        var toolPart = Assert.IsType<ToolUIPart>(first.ResponseMessage.Parts[1]);
        Assert.Equal("tool-weather", toolPart.Type);
        Assert.Equal("call-1", toolPart.ToolCallId);
        Assert.Equal(ToolUIPartState.OutputAvailable, toolPart.State);
        Assert.Equal("San Francisco", toolPart.Input!.Value.GetProperty("location").GetString());
        Assert.Equal("San Francisco", toolPart.Output!.Value.GetProperty("location").GetString());
        Assert.Equal(72, toolPart.Output!.Value.GetProperty("temperature").GetInt32());

        var second = uiCalls[1].ResponseMessage;
        Assert.Equal("assistant-1", second.Id);
        Assert.Equal(UIMessageRole.Assistant, second.Role);
        Assert.Equal("finish-step", second.Metadata!.Value.GetProperty("partType").GetString());
        Assert.Equal(5, second.Parts.Count);
        Assert.IsType<StepStartUIPart>(second.Parts[0]);
        Assert.Equal("tool-weather", second.Parts[1].Type);
        Assert.IsType<StepStartUIPart>(second.Parts[2]);
        Assert.Equal(
            "The tool returned 72°F.",
            Assert.IsType<ReasoningUIPart>(second.Parts[3]).Text
        );
        Assert.Equal(
            "It is 72°F in San Francisco.",
            Assert.IsType<TextUIPart>(second.Parts[4]).Text
        );
    }

    [Theory]
    [InlineData("stream")]
    [InlineData("response")]
    [UpstreamTest(
        Prefix + "should convert custom data parts through the %s helper",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_custom_data_parts_through_the_helper(string helper)
    {
        var model = CreateMockModel();
        var agent = new Agent(new AgentOptions { Model = model }, Client());
        var convertDataPartCalls = 0;
        UserContentPart? ConvertDataPart(DataUIPart part)
        {
            convertDataPartCalls++;
            return new TextContentPart("Project: " + part.Data.GetProperty("project").GetString());
        }

        var messages = new[]
        {
            new UIMessage(
                UIMessageRole.User,
                [
                    new TextUIPart("What project?"),
                    new DataUIPart("context", Json("""{"project":"AI SDK"}""")),
                ]
            )
            {
                Id = "1",
            },
        };
        if (helper == "stream")
        {
            await Drain(
                await UIMessageStreamConverter.CreateAgentUIStreamAsync(
                    new CreateAgentUIStreamOptions
                    {
                        Agent = agent,
                        UIMessages = messages,
                        ConvertDataPart = ConvertDataPart,
                    }
                )
            );
        }
        else
        {
            var response = await UIMessageStreamConverter.CreateAgentUIStreamResponse(
                new CreateAgentUIStreamResponseOptions
                {
                    Agent = agent,
                    UIMessages = messages,
                    ConvertDataPart = ConvertDataPart,
                }
            );
            await response.Body.ToArrayAsync();
        }

        Assert.Equal(1, convertDataPartCalls);
        var prompt = Assert.Single(Assert.Single(model.Calls).Prompt);
        var user = Assert.IsType<UserModelMessage>(prompt);
        Assert.Collection(
            user.Content,
            part => Assert.Equal("What project?", Assert.IsType<TextContentPart>(part).Text),
            part => Assert.Equal("Project: AI SDK", Assert.IsType<TextContentPart>(part).Text)
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "should ignore data parts without a converted result (%s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_data_parts_without_a_converted_result(bool hasCallback)
    {
        var model = CreateMockModel();
        var stream = await UIMessageStreamConverter.CreateAgentUIStreamAsync(
            new CreateAgentUIStreamOptions
            {
                Agent = new Agent(new AgentOptions { Model = model }, Client()),
                UIMessages =
                [
                    new UIMessage(
                        UIMessageRole.User,
                        [
                            new TextUIPart("Hello"),
                            new DataUIPart("context", Json("""{"project":"AI SDK"}""")),
                        ]
                    )
                    {
                        Id = "1",
                    },
                ],
                ConvertDataPart = hasCallback ? _ => null : null,
            }
        );
        await Drain(stream);

        var user = Assert.IsType<UserModelMessage>(
            Assert.Single(Assert.Single(model.Calls).Prompt)
        );
        Assert.Equal("Hello", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject stale terminal input for a currently available tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_stale_terminal_input_for_a_currently_available_tool()
    {
        var agent = new Agent(
            new AgentOptions { Model = CreateMockModel(), Tools = [CurrentTool()] },
            Client()
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            UIMessageStreamConverter.CreateAgentUIStreamAsync(
                new CreateAgentUIStreamOptions
                {
                    Agent = agent,
                    UIMessages =
                    [
                        new UIMessage(
                            UIMessageRole.Assistant,
                            [
                                new ToolUIPart("current", "call-1", ToolUIPartState.OutputAvailable)
                                {
                                    Input = Json("""{"previous":"value"}"""),
                                    Output = Json("""{"result":"done"}"""),
                                },
                            ]
                        )
                        {
                            Id = "assistant-1",
                        },
                    ],
                }
            )
        );

        Assert.Contains("Type validation failed for messages[0].parts[0].input", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject many malformed parts without running the agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = "9307549d51938172036744037d359735d214fe36"
    )]
    public async Task Should_reject_many_malformed_parts_without_running_the_agent()
    {
        var model = CreateMockModel();
        var uiMessages =
            """[{"id":"user-1","role":"user","parts":["""
            + string.Join(",", Enumerable.Repeat("{}", 2500))
            + "]}]";

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            UIMessageStreamConverter.CreateAgentUIStreamResponse(
                new CreateAgentUIStreamResponseOptions
                {
                    Agent = new Agent(new AgentOptions { Model = model }, Client()),
                    UIMessagesJson = Json(uiMessages),
                }
            )
        );

        Assert.IsType<Vercel.AI.ProviderUtils.TypeValidationException>(error);
        // the error message contains the validated value once:
        Assert.True(error.Message.Length < uiMessages.Length + 1000);
        Assert.Empty(model.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose unavailable terminal tools as dynamic parts to callbacks",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_expose_unavailable_terminal_tools_as_dynamic_parts_to_callbacks() =>
        AssertTerminalToolHistoryIsDynamic([CurrentTool()]);

    [Fact]
    [UpstreamTest(
        Prefix + "should expose terminal tool history as dynamic parts when tools are omitted",
        Coverage = UpstreamCoverage.Covered
    )]
    public Task Should_expose_terminal_tool_history_as_dynamic_parts_when_tools_are_omitted() =>
        AssertTerminalToolHistoryIsDynamic(null);

    private static async Task AssertTerminalToolHistoryIsDynamic(IReadOnlyList<Tool>? tools)
    {
        var agent = new Agent(
            new AgentOptions { Model = CreateMockModel(), Tools = tools },
            Client()
        );
        var ends = new List<UIMessageStreamOnEndEvent>();

        var stream = await UIMessageStreamConverter.CreateAgentUIStreamAsync(
            new CreateAgentUIStreamOptions
            {
                Agent = agent,
                UIMessages =
                [
                    new UIMessage(
                        UIMessageRole.Assistant,
                        [
                            new ToolUIPart("removed", "call-1", ToolUIPartState.OutputAvailable)
                            {
                                Input = Json("""{"previous":"value"}"""),
                                Output = Json("""{"result":"done"}"""),
                            },
                        ]
                    )
                    {
                        Id = "assistant-1",
                    },
                    UserText("user-1", "continue"),
                ],
                UIMessageStreamOptions = new UIMessageStreamOptions
                {
                    OnEnd = e =>
                    {
                        ends.Add(e);
                        return Task.CompletedTask;
                    },
                },
            }
        );

        await Drain(stream);

        var part = Assert.IsType<ToolUIPart>(Assert.Single(Assert.Single(ends).Messages[0].Parts));
        Assert.Equal("dynamic-tool", part.Type);
        Assert.True(part.IsDynamic);
        Assert.False(part.Dynamic);
        Assert.Equal("removed", part.ToolName);
        Assert.Equal("call-1", part.ToolCallId);
        Assert.Equal(ToolUIPartState.OutputAvailable, part.State);
        Assert.Equal("value", part.Input!.Value.GetProperty("previous").GetString());
        Assert.Equal("done", part.Output!.Value.GetProperty("result").GetString());
    }
}
