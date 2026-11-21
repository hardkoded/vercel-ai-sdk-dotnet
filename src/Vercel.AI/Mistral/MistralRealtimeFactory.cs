// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Credentials for one browser voice conversation. Maps to <c>MistralRealtimeSession</c>.</summary>
public sealed class MistralRealtimeSession
{
    /// <summary>Creates a session.</summary>
    public MistralRealtimeSession(string id, string participantToken, string serverUrl)
    {
        Id = id;
        ParticipantToken = participantToken;
        ServerUrl = serverUrl;
    }

    /// <summary>Session id. Maps to <c>id</c>.</summary>
    public string Id { get; }

    /// <summary>Token the browser joins with. Maps to <c>participantToken</c>.</summary>
    public string ParticipantToken { get; }

    /// <summary>LiveKit server URL. Maps to <c>serverUrl</c>.</summary>
    public string ServerUrl { get; }
}

/// <summary>
/// Creates Mistral voice agent models and manages their sessions. Maps to <c>MistralRealtimeFactory</c>, built by
/// <c>createMistralRealtimeFactory</c>. Reached through <see cref="MistralProvider.ExperimentalRealtime"/>.
/// </summary>
public sealed class MistralRealtimeFactory
{
    private readonly OpenAICompatibleProvider _provider;

    internal MistralRealtimeFactory(OpenAICompatibleProvider provider)
    {
        _provider = provider;
    }

    /// <summary>Creates a browser model. It does not load an API key. Maps to calling the factory with a model id.</summary>
    public MistralRealtimeModel Model(string modelId)
    {
        return new MistralRealtimeModel(modelId);
    }

    /// <summary>Server-only: creates a voice session using the Mistral API key. Maps to <c>createSession</c>.</summary>
    public async Task<MistralRealtimeSession> CreateSessionAsync(
        string model,
        RealtimeModelV4SessionConfig? sessionConfig = null,
        CancellationToken cancellationToken = default
    )
    {
        model = model ?? throw new ArgumentNullException(nameof(model));
        if (model.Trim().Length == 0)
        {
            throw new InvalidArgumentException(
                "model",
                model,
                "Mistral voice sessions require a model ID."
            );
        }

        var body = MistralRealtimeModelOptions.BuildMistralRealtimeSessionConfig(
            sessionConfig ?? new RealtimeModelV4SessionConfig(),
            model
        );
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "audio/sessions");
        var requestBody = body.ToJsonString();
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Post,
                url,
                ProviderExchange.Json(requestBody),
                _provider.CreateHeaders(),
                cancellationToken,
                (status, text) => MistralError.FailedResponse(status, text, url, body)
            )
            .ConfigureAwait(false);

        try
        {
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            return new MistralRealtimeSession(
                NonEmpty(root, "id"),
                NonEmpty(root, "participant_token"),
                NonEmpty(root, "server_url")
            );
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid Mistral realtime session response: expected id, participant_token and server_url."
            );
        }
    }

    /// <summary>Server-only: releases a voice session after the browser disconnects. Maps to <c>closeSession</c>.</summary>
    public async Task CloseSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default
    )
    {
        sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        if (sessionId.Trim().Length == 0 || sessionId == "." || sessionId == "..")
        {
            throw new InvalidArgumentException(
                "sessionId",
                sessionId,
                "Mistral voice sessions require a valid session ID."
            );
        }

        var url = ApiKeys.Combine(
            _provider.Options.BaseUrl,
            "audio/sessions/" + Uri.EscapeDataString(sessionId)
        );
        await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Delete,
                url,
                null,
                _provider.CreateHeaders(),
                cancellationToken,
                (status, text) => MistralError.FailedResponse(status, text, url, null)
            )
            .ConfigureAwait(false);
    }

    private static string NonEmpty(JsonElement root, string name)
    {
        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
            || value.GetString()!.Length == 0
        )
        {
            throw new FormatException(name);
        }

        return value.GetString()!;
    }
}
