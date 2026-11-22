// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>LiveKit stays inside the Mistral provider and is loaded only on connection. Maps to <c>MistralRealtimeTransport</c>.</summary>
public sealed class MistralRealtimeTransport : IRealtimeModelV4Transport
{
    private static readonly HttpClient s_defaultHttpClient = new();

    private readonly RealtimeModelV4TransportOptions _options;
    private readonly ILiveKitClient? _liveKit;
    private readonly HttpClient _httpClient;
    private readonly object _gate = new();
    private readonly CancellationTokenSource _abort = new();
    private readonly HashSet<string> _finalizedSegments = [];
    private readonly Queue<string> _finalizedOrder = new();
    private readonly HashSet<ILiveKitLocalAudioTrack> _ownedTracks = [];
    private readonly Dictionary<ILiveKitRemoteTrack, ILiveKitMediaElement> _remoteAudio = [];
    private readonly Dictionary<string, PendingTool> _pendingTools = [];

    private ILiveKitRoom? _room;
    private SessionCredentials? _session;
    private string? _api;
    private bool _started;
    private volatile bool _closed;
    private volatile bool _accepting = true;
    private bool _ready;
    private bool _failed;
    private volatile bool _reconnecting;
    private volatile bool _agentDisconnected;
    private volatile bool _agentCheckQueued;
    private Task? _setup;
    private Task? _cleanup;
    private Action? _removeAbortListener;
    private Action? _removeRoomListeners;
    private Task _incoming = Task.CompletedTask;
    private Task _deliveries = Task.CompletedTask;
    private List<RealtimeModelV4ServerEvent> _pendingEvents = [];
    private int _pendingCount;
    private long _pendingBytes;
    private int _captureVersion;
    private Task _captureOperations = Task.CompletedTask;
    private LiveKitMediaStream? _sourceStream;
    private Microphone? _microphone;
    private volatile bool _playbackEnabled = true;
    private volatile bool _agentSpeaking;

    /// <summary>
    /// Creates a transport. It opens no room, makes no request and acquires no media until
    /// <see cref="ConnectAsync"/>. <paramref name="liveKit"/> is the host's LiveKit client (upstream loads
    /// <c>livekit-client</c>) and <paramref name="httpClient"/> calls the application's session endpoint (upstream
    /// <c>fetch</c>).
    /// </summary>
    public MistralRealtimeTransport(
        RealtimeModelV4TransportOptions options,
        ILiveKitClient? liveKit = null,
        HttpClient? httpClient = null
    )
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _liveKit = liveKit;
        _httpClient = httpClient ?? s_defaultHttpClient;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(RealtimeModelV4TransportConnectOptions options)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        if (_started || _closed)
        {
            throw new InvalidOperationException(
                "Create a new Mistral transport for each connection."
            );
        }

        _started = true;
        _api = options.Api;
        var registration = options.AbortSignal.Register(Dispose);
        _removeAbortListener = registration.Dispose;
        if (options.AbortSignal.IsCancellationRequested)
        {
            Dispose();
            throw new OperationCanceledException(
                "Mistral voice connection cancelled.",
                options.AbortSignal
            );
        }

        _setup = Open(options.SessionConfig, options.Stream, options.Capture ?? true);
        try
        {
            await _setup.ConfigureAwait(false);
        }
        catch
        {
            try
            {
                await CloseAsync().ConfigureAwait(false);
            }
            catch (Exception cleanupError)
            {
                _options.OnError(cleanupError);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public Task SendEventAsync(
        RealtimeModelV4ClientEvent clientEvent,
        Func<bool>? shouldSend = null
    )
    {
        clientEvent = clientEvent ?? throw new ArgumentNullException(nameof(clientEvent));
        AssertOpen();
        if (shouldSend?.Invoke() == false)
        {
            return Task.CompletedTask;
        }

        switch (clientEvent.Type)
        {
            case "session-update":
                return UpdateSession(
                    clientEvent.Config
                        ?? throw new ArgumentException("session-update needs a config."),
                    shouldSend
                );
            case "input-audio-mute":
                return StopCaptureAsync();
            case "input-audio-unmute":
                return StartCaptureAsync();
            case "conversation-item-create":
                if (clientEvent.Item?.Type == "function-call-output")
                {
                    PendingTool? pending;
                    var callId = clientEvent.Item.CallId ?? string.Empty;
                    lock (_gate)
                    {
                        if (!_pendingTools.TryGetValue(callId, out pending))
                        {
                            throw new InvalidOperationException(
                                "Mistral function call is no longer pending."
                            );
                        }

                        _pendingTools.Remove(callId);
                    }

                    pending.Timer.Dispose();
                    pending.Result.TrySetResult(clientEvent.Item.Output ?? string.Empty);
                    return Task.CompletedTask;
                }

                break;
            case "session-close":
                return CloseAsync();
        }

        var functionality = clientEvent.Type + " in Mistral voice sessions";
        throw new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }

    /// <inheritdoc />
    public async Task StartCaptureAsync(RealtimeModelV4MediaStream? stream = null)
    {
        AssertOpen();
        var version = Interlocked.Increment(ref _captureVersion);
        if (stream != null)
        {
            _sourceStream =
                stream as LiveKitMediaStream
                ?? throw new ArgumentException(
                    "Mistral voice needs a LiveKitMediaStream.",
                    nameof(stream)
                );
        }

        var source = _sourceStream;
        ILiveKitLocalAudioTrack? owned = null;
        if (source == null)
        {
            var liveKit = LoadLiveKit();
            if (_closed || version != Volatile.Read(ref _captureVersion))
            {
                return;
            }

            owned = await liveKit.CreateLocalAudioTrackAsync().ConfigureAwait(false);
            lock (_gate)
            {
                _ownedTracks.Add(owned);
            }
        }

        var track = owned?.MediaStreamTrack
            ?? source?.GetAudioTracks().FirstOrDefault(t => t.IsLive);
        if (_closed || version != Volatile.Read(ref _captureVersion))
        {
            StopOwnedTrack(owned);
            return;
        }

        if (track == null)
        {
            StopOwnedTrack(owned);
            throw new InvalidOperationException("Mistral voice requires a live audio track.");
        }

        var room = _room;
        if (room == null)
        {
            StopOwnedTrack(owned);
            throw new InvalidOperationException("Mistral voice is not connected.");
        }

        Task operation;
        lock (_gate)
        {
            operation = Chain(_captureOperations, async () =>
            {
                try
                {
                    await ReleaseMicrophone().ConfigureAwait(false);
                    if (_closed || version != Volatile.Read(ref _captureVersion))
                    {
                        StopOwnedTrack(owned);
                        return;
                    }

                    await room
                        .LocalParticipant.PublishMicrophoneTrackAsync(track)
                        .ConfigureAwait(false);
                    if (_closed || version != Volatile.Read(ref _captureVersion))
                    {
                        await room
                            .LocalParticipant.UnpublishTrackAsync(track, false)
                            .ConfigureAwait(false);
                        StopOwnedTrack(owned);
                        return;
                    }

                    EventHandler onEnded = (_, _) =>
                    {
                        if (_microphone?.Track == track)
                        {
                            _options.OnCapturing(false);
                        }
                    };
                    track.Ended += onEnded;
                    _microphone = new Microphone(track, owned, () => track.Ended -= onEnded);
                    _options.OnCapturing(track.Enabled && !track.Muted);
                }
                catch
                {
                    StopOwnedTrack(owned);
                    throw;
                }
            });
            _captureOperations = Swallow(operation);
        }

        await operation.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task StopCaptureAsync()
    {
        Interlocked.Increment(ref _captureVersion);
        lock (_gate)
        {
            var operation = Chain(_captureOperations, ReleaseMicrophone);
            _captureOperations = Swallow(operation);
            return operation;
        }
    }

    /// <inheritdoc />
    public void StopPlayback()
    {
        _playbackEnabled = false;
        foreach (var element in RemoteElements())
        {
            element.Pause();
        }

        _options.OnPlaying(false);
    }

    /// <inheritdoc />
    public async Task ResumePlaybackAsync()
    {
        AssertOpen();
        _playbackEnabled = true;
        if (_room != null)
        {
            await _room.StartAudioAsync().ConfigureAwait(false);
        }

        await Task.WhenAll(RemoteElements().Select(element => element.PlayAsync()))
            .ConfigureAwait(false);
        if (!_closed)
        {
            _options.OnPlaying(_agentSpeaking);
        }
    }

    /// <inheritdoc />
    public async Task FinishAsync()
    {
        _accepting = false;
        Task incoming;
        lock (_gate)
        {
            incoming = _incoming;
        }

        await incoming.ConfigureAwait(false);
        Task deliveries;
        lock (_gate)
        {
            deliveries = _deliveries;
        }

        await deliveries.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task CloseAsync()
    {
        TaskCompletionSource<bool> completion;
        lock (_gate)
        {
            if (_cleanup != null)
            {
                return _cleanup;
            }

            // Cache cleanup before callbacks: consumers may dispose again while a media state
            // notification is being delivered.
            completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _cleanup = completion.Task;
        }

        Task? disconnected = null;
        Exception? syncError = null;
        try
        {
            _closed = true;
            _accepting = false;
            Interlocked.Increment(ref _captureVersion);
            _abort.Cancel();
            _removeAbortListener?.Invoke();
            _removeRoomListeners?.Invoke();
            List<PendingTool> tools;
            List<ILiveKitRemoteTrack> remoteTracks;
            List<ILiveKitLocalAudioTrack> owned;
            lock (_gate)
            {
                _pendingEvents = [];
                tools = [.. _pendingTools.Values];
                _pendingTools.Clear();
                remoteTracks = [.. _remoteAudio.Keys];
                owned = [.. _ownedTracks];
            }

            foreach (var pending in tools)
            {
                pending.Timer.Dispose();
                pending.Result.TrySetException(
                    new InvalidOperationException("Mistral voice session closed.")
                );
            }

            foreach (var track in remoteTracks)
            {
                RemoveRemoteTrack(track);
            }

            foreach (var track in owned)
            {
                StopOwnedTrack(track);
            }

            _microphone?.RemoveListener();
            _microphone = null;
            _options.OnCapturing(false);
            _options.OnPlaying(false);
            // false is essential: LiveKit must not stop caller-owned microphone tracks.
            disconnected = _room?.DisconnectAsync(false);
        }
        catch (Exception error)
        {
            syncError = error;
            try
            {
                disconnected ??= _room?.DisconnectAsync(false);
            }
            catch
            {
                // The first failure is the one reported.
            }
        }

        _ = RunCleanup(disconnected, completion, syncError);
        return completion.Task;
    }

    /// <summary>Closes the transport without waiting. Errors go to <c>OnError</c>. Maps to <c>dispose</c>.</summary>
    public void Dispose()
    {
        CloseAsync()
            .ContinueWith(
                task => _options.OnError(task.Exception!.GetBaseException()),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default
            );
    }

    /// <summary>Voice transcripts contain replacement text, rather than append-only deltas. Maps to <c>parseMistralRealtimeEvent</c>.</summary>
    /// <exception cref="Vercel.AI.Operations.InvalidResponseDataException">A known event has the wrong shape.</exception>
    internal static IReadOnlyList<RealtimeModelV4ServerEvent> ParseMistralRealtimeEvent(
        JsonElement raw
    )
    {
        var type =
            raw.ValueKind == JsonValueKind.Object
            && raw.TryGetProperty("type", out var typeValue)
            && typeValue.ValueKind == JsonValueKind.String
                ? typeValue.GetString()
                : null;
        switch (type)
        {
            case "transcription.segment":
            {
                var id = Required(raw, "id", JsonValueKind.String).GetString()!;
                var role = Required(raw, "role", JsonValueKind.String).GetString();
                var content = Required(raw, "content", JsonValueKind.String).GetString()!;
                var final = Required(raw, "final", JsonValueKind.True, JsonValueKind.False)
                    .GetBoolean();
                if (role != "user" && role != "assistant")
                {
                    throw Invalid(raw, "role");
                }

                if (!final)
                {
                    return [];
                }

                if (role == "user")
                {
                    return
                    [
                        new RealtimeModelV4ServerEvent("input-transcription-completed")
                        {
                            ItemId = id,
                            Transcript = content,
                            Raw = raw,
                        },
                    ];
                }

                return
                [
                    new RealtimeModelV4ServerEvent("response-created")
                    {
                        ResponseId = id,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("audio-transcript-delta")
                    {
                        ResponseId = id,
                        ItemId = id,
                        Delta = content,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("audio-transcript-done")
                    {
                        ResponseId = id,
                        ItemId = id,
                        Transcript = content,
                        Raw = raw,
                    },
                    new RealtimeModelV4ServerEvent("response-done")
                    {
                        ResponseId = id,
                        Status = "completed",
                        Raw = raw,
                    },
                ];
            }
            case "session.updated":
                return [new RealtimeModelV4ServerEvent("session-updated") { Raw = raw }];
            case "error":
            {
                var message = Required(raw, "message", JsonValueKind.String).GetString();
                var code = Required(raw, "code", JsonValueKind.String).GetString();
                Required(raw, "fatal", JsonValueKind.True, JsonValueKind.False);
                return
                [
                    new RealtimeModelV4ServerEvent("error")
                    {
                        Message = message,
                        Code = code,
                        Raw = raw,
                    },
                ];
            }
            default:
                return
                [
                    new RealtimeModelV4ServerEvent("custom")
                    {
                        RawType = type ?? "unknown",
                        Raw = raw,
                    },
                ];
        }
    }

    private static JsonElement Required(JsonElement raw, string name, params JsonValueKind[] kinds)
    {
        if (!raw.TryGetProperty(name, out var value) || !kinds.Contains(value.ValueKind))
        {
            throw Invalid(raw, name);
        }

        return value;
    }

    private static Vercel.AI.Operations.InvalidResponseDataException Invalid(
        JsonElement raw,
        string field
    )
    {
        return new Vercel.AI.Operations.InvalidResponseDataException(
            raw.GetRawText(),
            "Invalid Mistral realtime event: " + field + " is missing or has the wrong type."
        );
    }

    private static Task Chain(Task previous, Func<Task> next)
    {
        return Run();

        async Task Run()
        {
            await previous.ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }
    }

    /// <summary>A task that never faults: the chains it guards report errors themselves.</summary>
    private static Task Swallow(Task task)
    {
        return task.ContinueWith(
            _ => { },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    private static JsonNode SessionConfigToJson(RealtimeModelV4SessionConfig config)
    {
        var node = new JsonObject();
        SetString(node, "instructions", config.Instructions);
        SetString(node, "voice", config.Voice);
        if (config.OutputModalities != null)
        {
            node["outputModalities"] = new JsonArray([
                .. config.OutputModalities.Select(m => (JsonNode?)JsonValue.Create(m)),
            ]);
        }

        SetFormat(
            node,
            "inputAudioFormat",
            config.InputAudioFormatType,
            config.InputAudioFormatRate
        );
        SetFormat(
            node,
            "outputAudioFormat",
            config.OutputAudioFormatType,
            config.OutputAudioFormatRate
        );
        SetTranscription(node, "inputAudioTranscription", config.InputAudioTranscription);
        SetTranscription(node, "outputAudioTranscription", config.OutputAudioTranscription);
        if (config.TurnDetectionDisabled)
        {
            node["turnDetection"] = null;
        }
        else if (config.TurnDetection is { } turn)
        {
            var turnNode = new JsonObject { ["type"] = turn.Type };
            SetNumber(turnNode, "threshold", turn.Threshold);
            SetNumber(turnNode, "silenceDurationMs", turn.SilenceDurationMs);
            SetNumber(turnNode, "prefixPaddingMs", turn.PrefixPaddingMs);
            node["turnDetection"] = turnNode;
        }

        if (config.Tools != null)
        {
            var tools = new JsonArray();
            foreach (var tool in config.Tools)
            {
                var toolNode = new JsonObject { ["name"] = tool.Name };
                SetString(toolNode, "description", tool.Description);
                toolNode["parameters"] = JsonNode.Parse(tool.Parameters.GetRawText());
                tools.Add(toolNode);
            }

            node["tools"] = tools;
        }

        if (config.ProviderOptions != null)
        {
            node["providerOptions"] = config.ProviderOptions.DeepClone();
        }

        return node;
    }

    private static void SetString(JsonObject node, string name, string? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static void SetNumber(JsonObject node, string name, double? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static void SetFormat(JsonObject node, string name, string? type, int? rate)
    {
        if (type == null && rate == null)
        {
            return;
        }

        var format = new JsonObject();
        SetString(format, "type", type);
        SetNumber(format, "rate", rate);
        node[name] = format;
    }

    private static void SetTranscription(
        JsonObject node,
        string name,
        RealtimeModelV4AudioTranscription? transcription
    )
    {
        if (transcription == null)
        {
            return;
        }

        var value = new JsonObject();
        SetString(value, "model", transcription.Model);
        SetString(value, "language", transcription.Language);
        SetString(value, "prompt", transcription.Prompt);
        node[name] = value;
    }

    private static string TrimJson(JsonElement element)
    {
        return JsonNode.Parse(element.GetRawText())!.ToJsonString();
    }

    private static bool IsFinalTranscript(JsonElement raw, out string id)
    {
        id = string.Empty;
        if (
            raw.ValueKind != JsonValueKind.Object
            || !Is(raw, "type", "transcription.segment")
            || !raw.TryGetProperty("id", out var idValue)
            || idValue.ValueKind != JsonValueKind.String
            || !raw.TryGetProperty("role", out var role)
            || role.ValueKind != JsonValueKind.String
            || (role.GetString() != "user" && role.GetString() != "assistant")
            || !raw.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.String
            || !raw.TryGetProperty("final", out var final)
            || final.ValueKind != JsonValueKind.True
        )
        {
            return false;
        }

        id = idValue.GetString()!;
        return true;
    }

    private static bool Is(JsonElement raw, string name, string value)
    {
        return raw.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.String
            && property.GetString() == value;
    }

    private OperationCanceledException Cancelled()
    {
        return new OperationCanceledException("Mistral voice connection cancelled.", _abort.Token);
    }

    private ILiveKitClient LoadLiveKit()
    {
        return _liveKit
            ?? throw new InvalidOperationException(
                "Cannot load the LiveKit client. Pass an ILiveKitClient to use Mistral realtime voice connections."
            );
    }

    private async Task Open(
        RealtimeModelV4SessionConfig? sessionConfig,
        RealtimeModelV4MediaStream? stream,
        bool capture
    )
    {
        var liveKit = LoadLiveKit();
        AssertOpen();
        // Let an in-flight create request finish on cancellation, so its returned session can
        // still be deleted. A bounded timeout covers an unresponsive app.
        using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        {
            var body = new JsonObject();
            if (sessionConfig != null)
            {
                body["sessionConfig"] = SessionConfigToJson(sessionConfig);
            }

            using var content = new StringContent(
                body.ToJsonString(),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            using var response = await _httpClient
                .PostAsync(new Uri(_api!, UriKind.RelativeOrAbsolute), content, timeout.Token)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    "Failed to create Mistral voice session: " + (int)response.StatusCode
                );
            }

            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _session = SessionCredentials.Parse(text);
        }

        AssertOpen();

        var room = liveKit.CreateRoom();
        _room = room;
        var agentReady = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        Action<ILiveKitRemoteParticipant> onParticipantConnected = participant =>
        {
            if (participant.IsAgent)
            {
                _agentDisconnected = false;
                agentReady.TrySetResult(true);
            }
        };
        Action<ILiveKitRemoteParticipant> onParticipantDisconnected = participant =>
        {
            if (participant.IsAgent)
            {
                _agentDisconnected = true;
                CheckAgentDisconnect();
            }
        };
        Action onReconnecting = () => _reconnecting = true;
        Action onReconnected = () =>
        {
            _reconnecting = false;
            CheckAgentDisconnect();
        };
        Action onDisconnected = () =>
        {
            if (!_closed)
            {
                Fail(new InvalidOperationException("Mistral voice connection disconnected."));
            }
        };
        Action<ILiveKitRemoteTrack, ILiveKitRemoteParticipant> onTrackSubscribed = (
            track,
            participant
        ) =>
        {
            if (_closed || !participant.IsAgent || !track.IsAudio)
            {
                return;
            }

            var element = track.Attach();
            lock (_gate)
            {
                _remoteAudio[track] = element;
            }

            if (_playbackEnabled)
            {
                element
                    .PlayAsync()
                    .ContinueWith(
                        task =>
                        {
                            if (!_closed)
                            {
                                _options.OnError(task.Exception!.GetBaseException());
                            }
                        },
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.Default
                    );
            }
            else
            {
                element.Pause();
            }
        };
        Action<ILiveKitRemoteTrack> onTrackUnsubscribed = RemoveRemoteTrack;
        Action<byte[], ILiveKitRemoteParticipant?, string?> onDataReceived = (
            payload,
            participant,
            topic
        ) =>
        {
            if (topic != "voice_agent.events" || participant is not { IsAgent: true })
            {
                return;
            }

            Receive(payload);
        };
        room.ParticipantConnected += onParticipantConnected;
        room.ParticipantDisconnected += onParticipantDisconnected;
        room.Disconnected += onDisconnected;
        room.Reconnecting += onReconnecting;
        room.Reconnected += onReconnected;
        room.TrackSubscribed += onTrackSubscribed;
        room.TrackUnsubscribed += onTrackUnsubscribed;
        room.DataReceived += onDataReceived;
        _removeRoomListeners = () =>
        {
            room.ParticipantConnected -= onParticipantConnected;
            room.ParticipantDisconnected -= onParticipantDisconnected;
            room.Disconnected -= onDisconnected;
            room.Reconnecting -= onReconnecting;
            room.Reconnected -= onReconnected;
            room.TrackSubscribed -= onTrackSubscribed;
            room.TrackUnsubscribed -= onTrackUnsubscribed;
            room.DataReceived -= onDataReceived;
        };
        var connecting = room.ConnectAsync(_session.ServerUrl, _session.ParticipantToken);
        _ = connecting.ContinueWith(
            task =>
            {
                if (task.Status == TaskStatus.RanToCompletion && _closed)
                {
                    room.DisconnectAsync(false)
                        .ContinueWith(
                            t => _ = t.Exception,
                            CancellationToken.None,
                            TaskContinuationOptions.OnlyOnFaulted,
                            TaskScheduler.Default
                        );
                }
                else
                {
                    _ = task.Exception;
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
        await UntilAborted(connecting).ConfigureAwait(false);
        AssertOpen();
        room.LocalParticipant.RegisterRpcMethod("mistral.function_tool", HandleToolCall);
        if (FindAgent() != null)
        {
            agentReady.TrySetResult(true);
        }

        if (capture)
        {
            await UntilAborted(StartCaptureAsync(stream)).ConfigureAwait(false);
        }

        await UntilAborted(agentReady.Task).ConfigureAwait(false);
        AssertOpen();
        await _options
            .OnEvent(
                new RealtimeModelV4ServerEvent("session-created")
                {
                    SessionId = _session.Id,
                    Raw = JsonSerializer.SerializeToElement(
                        new Dictionary<string, string>
                        {
                            ["type"] = "session.created",
                            ["id"] = _session.Id,
                        }
                    ),
                }
            )
            .ConfigureAwait(false);
        AssertOpen();
        List<Task> deliveries = [];
        lock (_gate)
        {
            _ready = true;
            // Chain the buffered events before any later event can be delivered.
            foreach (var buffered in _pendingEvents)
            {
                var delivery = Deliver(_deliveries, buffered);
                _deliveries = Swallow(delivery);
                deliveries.Add(delivery);
            }

            _pendingEvents = [];
        }

        await Task.WhenAll(deliveries).ConfigureAwait(false);
    }

    private async Task UpdateSession(RealtimeModelV4SessionConfig config, Func<bool>? shouldSend)
    {
        var payload = MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(
            config,
            update: true
        );
        AssertOpen();
        if (shouldSend?.Invoke() == false)
        {
            return;
        }

        var agent = FindAgent()
            ?? throw new InvalidOperationException("Mistral voice agent is not connected.");
        var response = await _room!
            .LocalParticipant.PerformRpcAsync(
                agent.Identity,
                "mistral.session_update",
                payload.ToJsonString(),
                TimeSpan.FromMilliseconds(5000)
            )
            .ConfigureAwait(false);
        // Invalid JSON is an error. The reply is an acknowledgement, not another event: the server
        // also broadcasts session.updated, so delivering both would duplicate the update.
        _ = ParseJson(response);
    }

    private static JsonElement ParseJson(string text, bool redact = false)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }
        catch (JsonException error)
        {
            throw new Vercel.AI.Operations.InvalidResponseDataException(
                redact ? string.Empty : text,
                "Invalid JSON: " + error.Message
            );
        }
    }

    private async Task<string> HandleToolCall(LiveKitRpcInvocationData data)
    {
        AssertOpen();
        if (data.CallerIdentity != FindAgent()?.Identity)
        {
            throw new InvalidOperationException("Only the Mistral voice agent may call tools.");
        }

        var parsed = ParseJson(data.Payload);
        if (
            parsed.ValueKind != JsonValueKind.Object
            || !parsed.TryGetProperty("name", out var nameValue)
            || nameValue.ValueKind != JsonValueKind.String
            || (
                parsed.TryGetProperty("arguments", out var args)
                && args.ValueKind != JsonValueKind.Object
            )
        )
        {
            throw new Vercel.AI.Operations.InvalidResponseDataException(
                data.Payload,
                "Invalid Mistral function call."
            );
        }

        AssertOpen();
        var name = nameValue.GetString()!;
        var call = new JsonObject { ["name"] = name };
        parsed.TryGetProperty("arguments", out args);
        if (args.ValueKind == JsonValueKind.Object)
        {
            call["arguments"] = JsonNode.Parse(args.GetRawText());
        }

        var result = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        lock (_gate)
        {
            if (_pendingTools.ContainsKey(data.RequestId) || _pendingTools.Count >= 64)
            {
                throw new InvalidOperationException(
                    "Mistral voice has too many pending function calls."
                );
            }

            var timer = new Timer(
                _ =>
                {
                    lock (_gate)
                    {
                        _pendingTools.Remove(data.RequestId);
                    }

                    result.TrySetException(
                        new InvalidOperationException("Mistral function " + name + " timed out.")
                    );
                },
                null,
                data.ResponseTimeout,
                Timeout.InfiniteTimeSpan
            );
            _pendingTools[data.RequestId] = new PendingTool(result, timer);
        }

        // Observe a rejection before invoking application callbacks.
        _ = result.Task.ContinueWith(
            t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default
        );
        var raw = JsonSerializer.SerializeToElement(call);
        await Emit(
                new RealtimeModelV4ServerEvent("function-call-arguments-delta")
                {
                    ResponseId = data.RequestId,
                    ItemId = data.RequestId,
                    CallId = data.RequestId,
                    Delta = string.Empty,
                    Raw = raw,
                }
            )
            .ConfigureAwait(false);
        await Emit(
                new RealtimeModelV4ServerEvent("function-call-arguments-done")
                {
                    ResponseId = data.RequestId,
                    ItemId = data.RequestId,
                    CallId = data.RequestId,
                    Name = name,
                    Arguments = args.ValueKind == JsonValueKind.Object ? TrimJson(args) : "{}",
                    Raw = raw,
                }
            )
            .ConfigureAwait(false);
        return await result.Task.ConfigureAwait(false);
    }

    private async Task ReleaseMicrophone()
    {
        var microphone = _microphone;
        _microphone = null;
        if (microphone != null)
        {
            microphone.RemoveListener();
            try
            {
                if (_room != null)
                {
                    await _room
                        .LocalParticipant.UnpublishTrackAsync(microphone.Track, false)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                StopOwnedTrack(microphone.Owned);
                _options.OnCapturing(false);
            }
        }
        else
        {
            _options.OnCapturing(false);
        }
    }

    private void StopOwnedTrack(ILiveKitLocalAudioTrack? track)
    {
        bool removed;
        lock (_gate)
        {
            removed = track != null && _ownedTracks.Remove(track);
        }

        if (removed)
        {
            track!.Stop();
        }
    }

    private List<ILiveKitMediaElement> RemoteElements()
    {
        lock (_gate)
        {
            return [.. _remoteAudio.Values];
        }
    }

    private void Receive(byte[] payload)
    {
        if (!_accepting || _closed)
        {
            return;
        }

        bool overflow;
        lock (_gate)
        {
            overflow = _pendingCount >= 512 || _pendingBytes + payload.Length > 8 * 1024 * 1024;
            if (!overflow)
            {
                ++_pendingCount;
                _pendingBytes += payload.Length;
                _incoming = ProcessIncoming(_incoming, payload);
            }
        }

        if (overflow)
        {
            Fail(new InvalidOperationException("Mistral voice event queue exceeded its limit."));
        }
    }

    private async Task ProcessIncoming(Task previous, byte[] payload)
    {
        await previous.ConfigureAwait(false);
        try
        {
            if (_closed)
            {
                return;
            }

            var raw = ParseJson(System.Text.Encoding.UTF8.GetString(payload));
            if (IsFinalTranscript(raw, out var id))
            {
                lock (_gate)
                {
                    if (!_finalizedSegments.Add(id))
                    {
                        return;
                    }

                    _finalizedOrder.Enqueue(id);
                    if (_finalizedSegments.Count > 500)
                    {
                        _finalizedSegments.Remove(_finalizedOrder.Dequeue());
                    }
                }
            }

            if (
                raw.ValueKind == JsonValueKind.Object
                && Is(raw, "type", "agent.state.changed")
                && raw.TryGetProperty("state", out var state)
                && state.ValueKind == JsonValueKind.String
            )
            {
                _agentSpeaking = state.GetString() == "speaking";
                _options.OnPlaying(_playbackEnabled && _agentSpeaking);
            }

            foreach (var parsedEvent in ParseMistralRealtimeEvent(raw))
            {
                await Emit(parsedEvent).ConfigureAwait(false);
            }

            if (
                raw.ValueKind == JsonValueKind.Object
                && Is(raw, "type", "error")
                && raw.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                && raw.TryGetProperty("code", out var code)
                && code.ValueKind == JsonValueKind.String
                && raw.TryGetProperty("fatal", out var fatal)
                && fatal.ValueKind == JsonValueKind.True
            )
            {
                Fail(new InvalidOperationException(message.GetString()));
            }
        }
        catch (Exception error)
        {
            Fail(error);
        }
        finally
        {
            lock (_gate)
            {
                --_pendingCount;
                _pendingBytes -= payload.Length;
            }
        }
    }

    private Task Emit(RealtimeModelV4ServerEvent serverEvent)
    {
        if (_closed)
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            if (!_ready)
            {
                if (_pendingEvents.Count >= 512)
                {
                    throw new InvalidOperationException(
                        "Mistral voice startup event queue is full."
                    );
                }

                _pendingEvents.Add(serverEvent);
                return Task.CompletedTask;
            }

            var delivery = Deliver(_deliveries, serverEvent);
            _deliveries = Swallow(delivery);
            return delivery;
        }
    }

    private async Task Deliver(Task previous, RealtimeModelV4ServerEvent serverEvent)
    {
        await previous.ConfigureAwait(false);
        if (_closed)
        {
            return;
        }

        try
        {
            await _options.OnEvent(serverEvent).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            _options.OnError(error);
        }
    }

    private ILiveKitRemoteParticipant? FindAgent()
    {
        return _room?.RemoteParticipants.FirstOrDefault(participant => participant.IsAgent);
    }

    private void CheckAgentDisconnect()
    {
        if (!_agentDisconnected || _agentCheckQueued)
        {
            return;
        }

        _agentCheckQueued = true;
        _ = Task.Run(() =>
        {
            _agentCheckQueued = false;
            if (_closed || _reconnecting)
            {
                return;
            }

            if (FindAgent() != null)
            {
                _agentDisconnected = false;
            }
            else
            {
                Fail(new InvalidOperationException("Mistral voice agent disconnected."));
            }
        });
    }

    private void AssertOpen()
    {
        if (_closed || _abort.IsCancellationRequested)
        {
            throw new InvalidOperationException("Mistral voice connection is closed.");
        }
    }

    private async Task UntilAborted(Task operation)
    {
        if (_abort.IsCancellationRequested)
        {
            throw Cancelled();
        }

        var aborted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var registration = _abort.Token.Register(() => aborted.TrySetResult(true));
        if (await Task.WhenAny(operation, aborted.Task).ConfigureAwait(false) != operation)
        {
            _ = operation.ContinueWith(
                t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default
            );
            throw Cancelled();
        }

        await operation.ConfigureAwait(false);
    }

    private void Fail(Exception error)
    {
        lock (_gate)
        {
            if (_closed || _failed)
            {
                return;
            }

            _failed = true;
        }

        _accepting = false;
        _options.OnFatalError(error, null);
        Dispose();
    }

    private void RemoveRemoteTrack(ILiveKitRemoteTrack track)
    {
        ILiveKitMediaElement? element;
        lock (_gate)
        {
            if (!_remoteAudio.TryGetValue(track, out element))
            {
                return;
            }

            _remoteAudio.Remove(track);
        }

        element.Pause();
        element.ClearSource();
        track.Detach(element);
        element.Remove();
    }

    private async Task RunCleanup(
        Task? disconnected,
        TaskCompletionSource<bool> completion,
        Exception? syncError
    )
    {
        try
        {
            try
            {
                if (disconnected != null)
                {
                    await disconnected.ConfigureAwait(false);
                }
            }
            finally
            {
                if (_setup != null)
                {
                    try
                    {
                        await _setup.ConfigureAwait(false);
                    }
                    catch
                    {
                        // The connect call reports its own failure.
                    }
                }

                if (_session != null && _api != null)
                {
                    var separator = _api.Contains('?') ? '&' : '?';
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    using var response = await _httpClient
                        .DeleteAsync(
                            new Uri(
                                _api + separator + "sessionId=" + Uri.EscapeDataString(_session.Id),
                                UriKind.RelativeOrAbsolute
                            ),
                            timeout.Token
                        )
                        .ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Failed to close Mistral voice session: " + (int)response.StatusCode
                        );
                    }
                }
            }

            if (syncError != null)
            {
                completion.TrySetException(syncError);
            }
            else
            {
                completion.TrySetResult(true);
            }
        }
        catch (Exception error)
        {
            completion.TrySetException(syncError ?? error);
        }
    }

    private sealed record PendingTool(TaskCompletionSource<string> Result, Timer Timer);

    private sealed record Microphone(
        ILiveKitMediaTrack Track,
        ILiveKitLocalAudioTrack? Owned,
        Action RemoveListener
    );

    private sealed record SessionCredentials(string Id, string ParticipantToken, string ServerUrl)
    {
        public static SessionCredentials Parse(string text)
        {
            var raw = ParseJson(text, redact: true);
            return new SessionCredentials(
                NonEmpty(raw, "id"),
                NonEmpty(raw, "participantToken"),
                NonEmpty(raw, "serverUrl")
            );
        }

        private static string NonEmpty(JsonElement raw, string name)
        {
            if (
                raw.ValueKind == JsonValueKind.Object
                && raw.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.String
                && value.GetString() is { Length: > 0 } result
            )
            {
                return result;
            }

            throw new Vercel.AI.Operations.InvalidResponseDataException(
                string.Empty,
                "Invalid Mistral voice session: " + name + " is missing or has the wrong type."
            );
        }
    }
}
