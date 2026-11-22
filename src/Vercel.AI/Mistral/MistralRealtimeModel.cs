// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>
/// A managed Mistral voice agent with a provider-owned LiveKit transport. Maps to <c>MistralRealtimeModel</c>.
/// It has no WebSocket: <see cref="BuildUri"/> and <see cref="ConnectAsync"/> are unsupported.
/// </summary>
public sealed class MistralRealtimeModel : IProviderTransportRealtimeModel
{
    private readonly ILiveKitClient? _liveKit;
    private readonly HttpClient? _httpClient;

    /// <summary>
    /// Creates a model. <paramref name="liveKit"/> is the host's LiveKit client, which its transports use to join
    /// the voice room (upstream loads <c>livekit-client</c>). <paramref name="httpClient"/> calls the application's
    /// session endpoint (upstream <c>fetch</c>).
    /// </summary>
    public MistralRealtimeModel(
        string modelId,
        ILiveKitClient? liveKit = null,
        HttpClient? httpClient = null
    )
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _liveKit = liveKit;
        _httpClient = httpClient;
    }

    /// <summary>Specification version. Maps to <c>specificationVersion</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => "mistral.realtime";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Conversation semantics and supported transports. Maps to <c>capabilities</c>.</summary>
    public RealtimeCapabilities Capabilities { get; } =
        new()
        {
            Conversation = "turn-based",
            Transports = ["webrtc"],
            Connections = ["provider"],
            Startup = "session-update",
            Finalization = "transport-close",
        };

    /// <summary>Creates a provider-owned LiveKit transport without opening a connection. Maps to <c>createTransport</c>.</summary>
    public IRealtimeModelV4Transport CreateTransport(RealtimeModelV4TransportOptions options)
    {
        return new MistralRealtimeTransport(options, _liveKit, _httpClient);
    }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        throw Unsupported("a WebSocket connection");
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.FromException<IRealtimeSession>(Unsupported("a WebSocket connection"));
    }

    /// <summary>Normalizes a provider event. Maps to <c>parseServerEvent</c>.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
    {
        return MistralRealtimeTransport.ParseMistralRealtimeEvent(raw);
    }

    /// <summary>Always throws: client events go through the provider transport only. Maps to <c>serializeClientEvent</c>.</summary>
    /// <exception cref="UnsupportedFunctionalityException">Always.</exception>
    public JsonElement SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent)
    {
        clientEvent = clientEvent ?? throw new ArgumentNullException(nameof(clientEvent));
        throw Unsupported(clientEvent.Type + " outside its provider transport");
    }

    /// <summary>Builds the session configuration. Maps to <c>buildSessionConfig</c>.</summary>
    public JsonElement BuildSessionConfig(RealtimeModelV4SessionConfig config)
    {
        return JsonSerializer.SerializeToElement(
            MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(config, ModelId)
        );
    }

    private static UnsupportedFunctionalityException Unsupported(string what)
    {
        var functionality = "Mistral voice agent " + what;
        return new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }
}
