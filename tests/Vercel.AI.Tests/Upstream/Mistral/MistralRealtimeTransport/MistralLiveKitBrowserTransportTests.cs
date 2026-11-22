// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Mistral;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeTransport;

/// <summary>Port of <c>mistral-realtime-transport.test.ts</c> &gt; <c>Mistral LiveKit browser transport</c>.</summary>
public sealed class MistralLiveKitBrowserTransportTests : IAsyncLifetime
{
    private const string Id =
        "packages/mistral/src/realtime/mistral-realtime-transport.test.ts::Mistral LiveKit browser transport::";
    private const string Api = "/api/mistral-realtime";
    private const string Credentials =
        "{\"id\":\"session/1\",\"participantToken\":\"participant-token\",\"serverUrl\":\"wss://voice.example.com\"}";
    private const string Commit = "0912e0b1b937418b438344ff2098a57961a83787";

    private readonly List<global::Vercel.AI.Mistral.MistralRealtimeTransport> _transports = [];
    private readonly FakeLiveKit _livekit = new();
    private readonly ParityHandler _handler;
    private readonly HttpClient _http;
    private readonly FakeMicrophone _microphone = new();

    public MistralLiveKitBrowserTransportTests()
    {
        _handler = new ParityHandler(call =>
            call.Method == HttpMethod.Delete
                ? new HttpResponseMessage(HttpStatusCode.NoContent)
                : ParityHandler.Json(Credentials)
        );
        _http = new HttpClient(_handler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _livekit.CreateLocalAudioTrack = () =>
            Task.FromResult<ILiveKitLocalAudioTrack>(_microphone.Local);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_transports.Select(transport => transport.CloseAsync()));
        _http.Dispose();
    }

    private IEnumerable<ParityCall> Deletes =>
        _handler.Calls.Where(c => c.Method == HttpMethod.Delete);

    private sealed class Callbacks
    {
        public ConcurrentQueue<RealtimeModelV4ServerEvent> Events { get; } = new();
        public ConcurrentQueue<Exception> Fatal { get; } = new();
        public ConcurrentQueue<bool> Capturing { get; } = new();
        public ConcurrentQueue<bool> Playing { get; } = new();
        public ConcurrentQueue<Exception?> Closed { get; } = new();
    }

    private (
        global::Vercel.AI.Mistral.MistralRealtimeTransport Transport,
        Callbacks Callbacks
    ) Create(
        Func<RealtimeModelV4ServerEvent, Task>? onEvent = null,
        Action<bool, global::Vercel.AI.Mistral.MistralRealtimeTransport>? onCapturing = null
    )
    {
        var callbacks = new Callbacks();
        global::Vercel.AI.Mistral.MistralRealtimeTransport? transport = null;
        var options = new RealtimeModelV4TransportOptions(
            async ev =>
            {
                callbacks.Events.Enqueue(ev);
                if (onEvent != null)
                {
                    await onEvent(ev);
                }
            },
            _ => { },
            (error, _) => callbacks.Fatal.Enqueue(error),
            error => callbacks.Closed.Enqueue(error),
            value =>
            {
                callbacks.Capturing.Enqueue(value);
                onCapturing?.Invoke(value, transport!);
            },
            value => callbacks.Playing.Enqueue(value)
        );
        transport = new global::Vercel.AI.Mistral.MistralRealtimeTransport(
            options,
            _livekit,
            _http
        );
        _transports.Add(transport);
        return (transport, callbacks);
    }

    private static Task Connect(
        global::Vercel.AI.Mistral.MistralRealtimeTransport transport,
        bool capture = false,
        CancellationToken abort = default
    ) =>
        transport.ConnectAsync(
            new RealtimeModelV4TransportConnectOptions
            {
                Api = Api,
                Capture = capture,
                AbortSignal = abort,
            }
        );

    private void Receive(
        object payload,
        ILiveKitRemoteParticipant? participant = null,
        string topic = "voice_agent.events"
    )
    {
        _livekit
            .Rooms[0]
            .EmitDataReceived(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)),
                participant ?? FakeLiveKit.Agent,
                topic
            );
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for a condition.");
            await Task.Delay(10);
        }
    }

    private static void ThrowsSynchronously(Func<Task> action)
    {
        Assert.Throws<Vercel.AI.Operations.UnsupportedFunctionalityException>(() =>
        {
            _ = action();
        });
    }

    private static Task Flush() => Task.Delay(100);

    private static string[] Types(Callbacks callbacks) => [.. callbacks.Events.Select(e => e.Type)];

    private static LiveKitRpcInvocationData Rpc(string caller, string payload) =>
        new()
        {
            RequestId = "rpc-1",
            CallerIdentity = caller,
            Payload = payload,
            ResponseTimeout = TimeSpan.FromSeconds(5),
        };

    [Fact]
    [UpstreamTest(
        Id + "does not open a room or acquire media until connect",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Does_not_open_a_room_or_acquire_media_until_connect()
    {
        Create();
        Assert.Empty(_livekit.Rooms);
        Assert.Empty(_handler.Calls);
        Assert.Equal(0, _livekit.CreateLocalAudioTrackCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "creates a session through the application and connects with its credentials",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Creates_a_session_through_the_application_and_connects_with_its_credentials()
    {
        var (transport, callbacks) = Create();
        await transport.ConnectAsync(
            new RealtimeModelV4TransportConnectOptions
            {
                Api = Api,
                SessionConfig = new RealtimeModelV4SessionConfig
                {
                    Instructions = "Keep answers short.",
                },
                Capture = false,
            }
        );
        var post = Assert.Single(_handler.Calls);
        Assert.Equal(HttpMethod.Post, post.Method);
        Assert.Equal(Api, post.Uri.AbsolutePath);
        Assert.Equal("{\"sessionConfig\":{\"instructions\":\"Keep answers short.\"}}", post.Text);
        Assert.Equal(
            ("wss://voice.example.com", "participant-token"),
            Assert.Single(_livekit.Rooms[0].ConnectCalls)
        );
        var created = Assert.Single(callbacks.Events);
        Assert.Equal("session-created", created.Type);
        Assert.Equal("session/1", created.SessionId);
        Assert.Equal(
            "{\"type\":\"session.created\",\"id\":\"session/1\"}",
            created.Raw!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "waits for the agent before marking the session ready",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Waits_for_the_agent_before_marking_the_session_ready()
    {
        _livekit.WithAgent = false;
        var (transport, callbacks) = Create();
        var connecting = Connect(transport);
        await WaitFor(() => _livekit.Rooms.Count == 1);
        await Flush();
        Assert.Empty(callbacks.Events);
        _livekit.Rooms[0].Participants.Add(FakeLiveKit.Agent);
        _livekit.Rooms[0].EmitParticipantConnected(FakeLiveKit.Agent);
        await connecting;
        Assert.Contains(callbacks.Events, e => e.Type == "session-created");
    }

    [Fact]
    [UpstreamTest(
        Id + "publishes microphone audio and releases owned media on close",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Publishes_microphone_audio_and_releases_owned_media_on_close()
    {
        var (transport, callbacks) = Create();
        await Connect(transport, true);
        var room = _livekit.Rooms[0];
        Assert.Same(_microphone.Track, Assert.Single(room.Local.Published));
        Assert.True(callbacks.Capturing.Last());
        await transport.CloseAsync();
        Assert.Equal(1, _microphone.Local.StopCalls);
        Assert.Equal([false], room.DisconnectCalls);
        Assert.Empty(callbacks.Closed);
        Assert.Equal(HttpMethod.Delete, _handler.Calls.Last().Method);
        Assert.Equal("sessionId=session%2F1", _handler.Calls.Last().Uri.Query.TrimStart('?'));
        await transport.CloseAsync();
        Assert.Single(Deletes);
    }

    [Fact]
    [UpstreamTest(
        Id + "never stops caller-owned microphone tracks",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Never_stops_caller_owned_microphone_tracks()
    {
        var (transport, _) = Create();
        await transport.ConnectAsync(
            new RealtimeModelV4TransportConnectOptions { Api = Api, Stream = _microphone.Stream }
        );
        await transport.StopCaptureAsync();
        Assert.Equal(
            (_microphone.Track, false),
            Assert.Single(_livekit.Rooms[0].Local.Unpublished)
        );
        await transport.StartCaptureAsync();
        await transport.CloseAsync();
        Assert.Equal(0, _microphone.Track.StopCalls);
        Assert.Equal(0, _livekit.CreateLocalAudioTrackCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "keeps cleanup idempotent when a media callback closes again",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Keeps_cleanup_idempotent_when_a_media_callback_closes_again()
    {
        var (transport, _) = Create(
            onCapturing: (value, self) =>
            {
                if (!value)
                {
                    self.Dispose();
                }
            }
        );
        await Connect(transport);
        await transport.CloseAsync();
        Assert.Single(_livekit.Rooms[0].DisconnectCalls);
        Assert.Single(Deletes);
    }

    [Fact]
    [UpstreamTest(
        Id + "deletes a session returned after cancellation of its POST",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Deletes_a_session_returned_after_cancellation_of_its_POST()
    {
        var response = new TaskCompletionSource<HttpResponseMessage>();
        _handler.RespondAsync = call =>
            call.Method == HttpMethod.Delete
                ? Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent))
                : response.Task;
        using var abort = new CancellationTokenSource();
        var (transport, callbacks) = Create();
        var connecting = Connect(transport, abort: abort.Token);
        await WaitFor(() => _handler.Calls.Count == 1);
        abort.Cancel();
        var closing = transport.CloseAsync();
        await Flush();
        Assert.False(closing.IsCompleted);
        response.SetResult(ParityHandler.Json(Credentials));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => connecting);
        Assert.Contains("closed", error.Message);
        await closing;
        Assert.Empty(_livekit.Rooms);
        Assert.Equal(0, _livekit.CreateLocalAudioTrackCalls);
        Assert.Empty(callbacks.Events);
        Assert.Equal(HttpMethod.Delete, _handler.Calls.Last().Method);
    }

    [Fact]
    [UpstreamTest(
        Id + "closes while microphone permission is pending and stops a late owned track",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Closes_while_microphone_permission_is_pending_and_stops_a_late_owned_track()
    {
        var media = new TaskCompletionSource<ILiveKitLocalAudioTrack>();
        _livekit.CreateLocalAudioTrack = () => media.Task;
        var (transport, callbacks) = Create();
        var connecting = Connect(transport, true);
        await WaitFor(() => _livekit.CreateLocalAudioTrackCalls == 1);
        await transport.CloseAsync();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connecting);
        Assert.Contains("cancelled", error.Message);
        Assert.Equal(HttpMethod.Delete, _handler.Calls.Last().Method);
        Assert.Contains("sessionId=", _handler.Calls.Last().Uri.Query);
        media.SetResult(_microphone.Local);
        await WaitFor(() => _microphone.Local.StopCalls == 1);
        await Flush();
        Assert.Equal(1, _microphone.Local.StopCalls);
        Assert.Empty(_livekit.Rooms[0].Local.Published);
        Assert.Empty(callbacks.Events);
    }

    [Fact]
    [UpstreamTest(
        Id + "cleans up a session when microphone acquisition fails",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Cleans_up_a_session_when_microphone_acquisition_fails()
    {
        _livekit.CreateLocalAudioTrack = () =>
            Task.FromException<ILiveKitLocalAudioTrack>(
                new InvalidOperationException("Microphone permission denied")
            );
        var (transport, _) = Create();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Connect(transport, true)
        );
        Assert.Contains("permission denied", error.Message);
        Assert.Equal([false], _livekit.Rooms[0].DisconnectCalls);
        Assert.Equal(HttpMethod.Delete, _handler.Calls.Last().Method);
        Assert.Contains("sessionId=", _handler.Calls.Last().Uri.Query);
    }

    [Fact]
    [UpstreamTest(
        Id + "maps final transcripts without accumulating revised partial text or duplicate finals",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Maps_final_transcripts_without_accumulating_revised_partial_text_or_duplicate_finals()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        foreach (
            var (content, final) in new[]
            {
                ("I scream", false),
                ("Ice cream", false),
                ("Ice cream.", true),
                ("Ice cream.", true),
            }
        )
        {
            Receive(
                new
                {
                    type = "transcription.segment",
                    id = "user-1",
                    role = "user",
                    content,
                    final,
                }
            );
        }

        Receive(
            new
            {
                type = "transcription.segment",
                id = "assistant-1",
                role = "assistant",
                content = "Which flavor?",
                final = true,
            }
        );
        await transport.FinishAsync();
        Assert.Equal(
            [
                "session-created",
                "input-transcription-completed",
                "response-created",
                "audio-transcript-delta",
                "audio-transcript-done",
                "response-done",
            ],
            Types(callbacks)
        );
        Assert.Contains(callbacks.Events, e => e.Transcript == "Ice cream.");
        Assert.Contains(callbacks.Events, e => e.Transcript == "Which flavor?");
    }

    [Fact]
    [UpstreamTest(
        Id + "ignores data from other topics and non-agent participants",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Ignores_data_from_other_topics_and_non_agent_participants()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        Receive(new { type = "session.updated" }, FakeLiveKit.Agent, "other-topic");
        Receive(new { type = "session.updated" }, new FakeAgent("user", false));
        await transport.FinishAsync();
        Assert.Single(callbacks.Events);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "routes client tool execution through normalized events and returns the submitted output",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Routes_client_tool_execution_through_normalized_events_and_returns_the_submitted_output()
    {
        global::Vercel.AI.Mistral.MistralRealtimeTransport? self = null;
        var (transport, callbacks) = Create(onEvent: async ev =>
        {
            if (ev.Type == "function-call-arguments-done")
            {
                await self!.SendEventAsync(
                    new RealtimeModelV4ClientEvent("conversation-item-create")
                    {
                        Item = new RealtimeModelV4ConversationItem("function-call-output")
                        {
                            CallId = ev.CallId,
                            Output = "{\"temperature\":20}",
                        },
                    }
                );
            }
        });
        self = transport;
        await Connect(transport);
        var invoke = _livekit.Rooms[0].Local.Rpc["mistral.function_tool"];
        var result = await invoke(
            Rpc(
                FakeLiveKit.Agent.Identity,
                "{\"name\":\"weather\",\"arguments\":{\"city\":\"Paris\"}}"
            )
        );
        Assert.Equal("{\"temperature\":20}", result);
        var done = Assert.Single(callbacks.Events, e => e.Type == "function-call-arguments-done");
        Assert.Equal("weather", done.Name);
        Assert.Equal("rpc-1", done.CallId);
        Assert.Equal("{\"city\":\"Paris\"}", done.Arguments);
        Assert.DoesNotContain(callbacks.Events, e => e.Type == "response-done");
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects tool RPCs from participants other than the agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_tool_RPCs_from_participants_other_than_the_agent()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        var invoke = _livekit.Rooms[0].Local.Rpc["mistral.function_tool"];
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invoke(Rpc("another-client", "{\"name\":\"weather\"}"))
        );
        Assert.Contains("Only the Mistral voice agent", error.Message);
        Assert.Single(callbacks.Events);
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects pending tool results when closed",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_pending_tool_results_when_closed()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        var pending = _livekit
            .Rooms[0]
            .Local.Rpc["mistral.function_tool"](
                Rpc(FakeLiveKit.Agent.Identity, "{\"name\":\"weather\"}")
            );
        await WaitFor(() => callbacks.Events.Count == 3);
        await transport.CloseAsync();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => pending);
        Assert.Contains("session closed", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "sends supported updates through RPC and rejects voice changes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Sends_supported_updates_through_RPC_and_rejects_voice_changes()
    {
        var (transport, _) = Create();
        await Connect(transport);
        await transport.SendEventAsync(
            new RealtimeModelV4ClientEvent("session-update")
            {
                Config = new RealtimeModelV4SessionConfig { Instructions = "Speak French." },
            }
        );
        Assert.Equal(
            (
                FakeLiveKit.Agent.Identity,
                "mistral.session_update",
                "{\"instructions\":\"Speak French.\"}",
                TimeSpan.FromMilliseconds(5000)
            ),
            Assert.Single(_livekit.Rooms[0].Local.RpcCalls)
        );
        var error =
            await Assert.ThrowsAsync<Vercel.AI.Operations.UnsupportedFunctionalityException>(() =>
                transport.SendEventAsync(
                    new RealtimeModelV4ClientEvent("session-update")
                    {
                        Config = new RealtimeModelV4SessionConfig { Voice = "different-voice" },
                    }
                )
            );
        Assert.Contains("live update of voice", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects unsupported input and response commands synchronously",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Rejects_unsupported_input_and_response_commands_synchronously()
    {
        var (transport, _) = Create();
        await Connect(transport);
        ThrowsSynchronously(() =>
            transport.SendEventAsync(new RealtimeModelV4ClientEvent("response-create"))
        );
        ThrowsSynchronously(() =>
            transport.SendEventAsync(
                new RealtimeModelV4ClientEvent("input-audio-append") { Audio = "AAAA" }
            )
        );
        ThrowsSynchronously(() =>
            transport.SendEventAsync(
                new RealtimeModelV4ClientEvent("conversation-item-create")
                {
                    Item = new RealtimeModelV4ConversationItem("text-message")
                    {
                        Role = "user",
                        Text = "Hi",
                    },
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "plays agent tracks and removes playback resources on close",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Plays_agent_tracks_and_removes_playback_resources_on_close()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        var element = new FakeElement();
        var track = new FakeRemoteTrack(element);
        _livekit.Rooms[0].EmitTrackSubscribed(track, FakeLiveKit.Agent);
        Assert.Equal(1, element.PlayCalls);
        Receive(new { type = "agent.state.changed", state = "speaking" });
        await WaitFor(() => callbacks.Playing.Count > 0 && callbacks.Playing.Last());
        transport.StopPlayback();
        Assert.Equal(1, element.PauseCalls);
        await transport.ResumePlaybackAsync();
        Assert.Equal(2, element.PlayCalls);
        await transport.CloseAsync();
        Assert.True(element.SourceCleared);
        Assert.Equal(1, element.RemoveCalls);
        Assert.Same(element, Assert.Single(track.Detached));
        Assert.False(callbacks.Playing.Last());
    }

    [Fact]
    [UpstreamTest(
        Id + "cleans up after a fatal agent error",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Cleans_up_after_a_fatal_agent_error()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        Receive(
            new
            {
                type = "error",
                message = "The speech model failed.",
                code = "tts_failed",
                fatal = true,
            }
        );
        await WaitFor(() => callbacks.Fatal.Count == 1);
        await transport.CloseAsync();
        Assert.Equal(HttpMethod.Delete, _handler.Calls.Last().Method);
        Assert.Contains("sessionId=", _handler.Calls.Last().Uri.Query);
    }

    [Fact]
    [UpstreamTest(
        Id + "allows the agent to rejoin while LiveKit reconnects",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Allows_the_agent_to_rejoin_while_LiveKit_reconnects()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        var room = _livekit.Rooms[0];
        room.EmitReconnecting();
        room.Participants.Remove(FakeLiveKit.Agent);
        room.EmitParticipantDisconnected(FakeLiveKit.Agent);
        await Flush();
        Assert.Empty(callbacks.Fatal);
        Assert.Empty(room.DisconnectCalls);
        room.Participants.Add(FakeLiveKit.Agent);
        room.EmitParticipantConnected(FakeLiveKit.Agent);
        room.EmitReconnected();
        await Flush();
        Assert.Empty(callbacks.Fatal);
    }

    [Fact]
    [UpstreamTest(
        Id + "closes if the agent remains absent after reconnection",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Closes_if_the_agent_remains_absent_after_reconnection()
    {
        var (transport, callbacks) = Create();
        await Connect(transport);
        var room = _livekit.Rooms[0];
        room.EmitReconnecting();
        room.Participants.Remove(FakeLiveKit.Agent);
        room.EmitParticipantDisconnected(FakeLiveKit.Agent);
        await Flush();
        room.EmitReconnected();
        await WaitFor(() => callbacks.Fatal.Count == 1);
        await transport.CloseAsync();
        Assert.Equal([false], room.DisconnectCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "drains accepted events before finish resolves",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Drains_accepted_events_before_finish_resolves()
    {
        var delivery = new TaskCompletionSource();
        var (transport, callbacks) = Create(onEvent: async ev =>
        {
            if (ev.Type == "session-updated")
            {
                await delivery.Task;
            }
        });
        await Connect(transport);
        Receive(new { type = "session.updated" });
        var finishing = transport.FinishAsync();
        await Flush();
        Assert.False(finishing.IsCompleted);
        Receive(new { type = "ignored-after-finish" });
        delivery.SetResult();
        await finishing;
        Assert.Equal(["session-created", "session-updated"], Types(callbacks));
    }
}
