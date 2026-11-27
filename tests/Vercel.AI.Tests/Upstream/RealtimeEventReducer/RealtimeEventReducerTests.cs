// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Reducer = Vercel.AI.RealtimeEventReducer;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.RealtimeEventReducer;

/// <summary>Port of <c>realtime-event-reducer.test.ts</c> &gt; <c>RealtimeEventReducer</c>.</summary>
public sealed class RealtimeEventReducerTests
{
    private const string Prefix =
        "packages/ai/src/realtime/realtime-event-reducer.test.ts::RealtimeEventReducer::";

    public static TheoryData<string> UnrelatedEventTypes =>
        [
            "session-started",
            "session-closed",
            "session-usage",
            "audio-chunk",
            "transcript-fragment",
            "delegation-created",
            "command-acknowledged",
        ];

    private static RealtimeModelV4ServerEvent Event(string type) =>
        type switch
        {
            "session-started" => new(type) { SessionId = "live-1" },
            "session-closed" => new(type)
            {
                Usage = new RealtimeModelV4Usage { Seconds = 10 },
                Reason = "closed",
            },
            "session-usage" => new(type) { Usage = new RealtimeModelV4Usage { Seconds = 5 } },
            "audio-chunk" => new(type) { Delta = "audio" },
            "transcript-fragment" => new(type)
            {
                Speaker = "assistant",
                Delta = "fragment",
                StartMs = 0,
                EndMs = 100,
            },
            "delegation-created" => new(type) { DelegationId = "delegation-1" },
            "command-acknowledged" => new(type) { Command = "session.update" },
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    private static RealtimeModelV4ServerEvent TextDelta(string delta) =>
        new("text-delta")
        {
            ResponseId = "response-1",
            ItemId = "item-1",
            Delta = delta,
        };

    private static async Task<RealtimeState> Reduce(
        Reducer reducer,
        RealtimeState state,
        RealtimeModelV4ServerEvent serverEvent
    ) => (await reducer.ReduceServerEventAsync(state, serverEvent)).State;

    [Theory]
    [MemberData(nameof(UnrelatedEventTypes))]
    [UpstreamTest(
        Prefix + "records $type without affecting an existing turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_type_without_affecting_an_existing_turn(string type)
    {
        var reducer = new Reducer();
        var serverEvent = Event(type);
        var state = (
            await reducer.ReduceServerEventAsync(
                Reducer.CreateInitialRealtimeState() with
                {
                    Status = RealtimeStatus.Connected,
                    IsPlaying = true,
                },
                TextDelta("Hel")
            )
        ).State;

        var result = await reducer.ReduceServerEventAsync(state, serverEvent);

        Assert.Empty(result.Effects);
        Assert.Equal(state.Status, result.State.Status);
        Assert.Equal(state.IsPlaying, result.State.IsPlaying);
        Assert.Equal([.. state.Events, serverEvent], result.State.Events);
        Assert.Same(state.Messages, result.State.Messages);

        var continued = await Reduce(reducer, result.State, TextDelta("lo"));
        var message = Assert.Single(continued.Messages);
        var part = Assert.IsType<TextUIPart>(Assert.Single(message.Parts));
        Assert.Equal("Hello", part.Text);
        Assert.Equal("streaming", part.State);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "assembles streamed text into UI messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Assembles_streamed_text_into_UI_messages()
    {
        var reducer = new Reducer();
        var state = Reducer.CreateInitialRealtimeState();
        state = await Reduce(reducer, state, TextDelta("Hel"));
        state = await Reduce(reducer, state, TextDelta("lo"));
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("text-done")
            {
                ResponseId = "response-1",
                ItemId = "item-1",
                Text = "Hello",
            }
        );

        var message = Assert.Single(state.Messages);
        Assert.Equal(UIMessageRole.Assistant, message.Role);
        var part = Assert.IsType<TextUIPart>(Assert.Single(message.Parts));
        Assert.Equal("Hello", part.Text);
        Assert.Equal("done", part.State);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps tool names available when adding tool output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Keeps_tool_names_available_when_adding_tool_output()
    {
        var reducer = new Reducer();
        var state = Reducer.CreateInitialRealtimeState();
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("function-call-arguments-delta")
            {
                ResponseId = "response-1",
                ItemId = "item-1",
                CallId = "call-1",
                Delta = "{\"city\":\"Paris\"}",
            }
        );

        var result = await reducer.ReduceServerEventAsync(
            state,
            new RealtimeModelV4ServerEvent("function-call-arguments-done")
            {
                ResponseId = "response-1",
                ItemId = "item-1",
                CallId = "call-1",
                Name = "getWeather",
                Arguments = "{\"city\":\"Paris\"}",
            }
        );

        var effect = Assert.Single(result.Effects, e => e.Type == "tool-call");
        Assert.Equal("call-1", effect.CallId);
        Assert.Equal("getWeather", effect.Name);
        Assert.Equal("{\"city\":\"Paris\"}", effect.Args!.Value.GetRawText());
        Assert.Equal("{\"city\":\"Paris\"}", effect.RawArguments);

        var output = reducer.AddToolOutput(
            result.State,
            "call-1",
            JsonSerializer.SerializeToElement(new { temperature = 22 })
        );

        Assert.Equal("call-1", output.Output.CallId);
        Assert.Equal("getWeather", output.Output.Name);
        Assert.Equal("{\"temperature\":22}", output.Output.Output);
        var tool = Assert.IsType<ToolUIPart>(Assert.Single(output.State.Messages[0].Parts));
        Assert.Equal("dynamic-tool", tool.Type);
        Assert.Equal("getWeather", tool.ToolName);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal(ToolUIPartState.OutputAvailable, tool.State);
        Assert.Equal("{\"city\":\"Paris\"}", tool.Input!.Value.GetRawText());
        Assert.Equal("{\"temperature\":22}", tool.Output!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "inserts delayed input transcriptions before the assistant response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Inserts_delayed_input_transcriptions_before_the_assistant_response()
    {
        var reducer = new Reducer();
        var state = Reducer.CreateInitialRealtimeState();
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("audio-committed") { ItemId = "user-item-1" }
        );
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("text-delta")
            {
                ResponseId = "response-1",
                ItemId = "assistant-item-1",
                Delta = "Yes, I can hear you.",
            }
        );
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("text-done")
            {
                ResponseId = "response-1",
                ItemId = "assistant-item-1",
                Text = "Yes, I can hear you.",
            }
        );
        state = await Reduce(
            reducer,
            state,
            new RealtimeModelV4ServerEvent("input-transcription-completed")
            {
                ItemId = "user-item-1",
                Transcript = "Can you hear me?",
            }
        );

        Assert.Equal(2, state.Messages.Count);
        Assert.Equal("user-user-item-1", state.Messages[0].Id);
        Assert.Equal(UIMessageRole.User, state.Messages[0].Role);
        var user = Assert.IsType<TextUIPart>(Assert.Single(state.Messages[0].Parts));
        Assert.Equal("Can you hear me?", user.Text);
        Assert.Equal("done", user.State);
        Assert.Equal(UIMessageRole.Assistant, state.Messages[1].Role);
        var assistant = Assert.IsType<TextUIPart>(Assert.Single(state.Messages[1].Parts));
        Assert.Equal("Yes, I can hear you.", assistant.Text);
        Assert.Equal("done", assistant.State);
    }
}
