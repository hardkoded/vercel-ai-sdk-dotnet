// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Options of <see cref="OpenAIProvider.RealtimeModel(string, OpenAIRealtimeOptions?)"/>. Maps to <c>OpenAIRealtimeOptions</c>.</summary>
public sealed class OpenAIRealtimeOptions
{
    /// <summary>Overrides model id routing, including for early-access models: <c>live</c> or <c>realtime</c>.</summary>
    public string? Api { get; set; }
}

/// <summary>Routes a model id to the Live or the Realtime API. Maps to <c>createOpenAIRealtimeFactory</c>.</summary>
internal static class OpenAIRealtimeFactory
{
    /// <summary>Creates the model for <paramref name="modelId"/>.</summary>
    internal static IRealtimeModel CreateModel(OpenAIProvider provider, string modelId, OpenAIRealtimeOptions? options)
    {
        return ResolveRealtimeApi(modelId, options) == "live"
            ? new OpenAIRealtimeModelLive(provider, modelId)
            : new OpenAIRealtimeModel(provider, modelId);
    }

    /// <summary>
    /// Mints a client secret for a Realtime model. A Live model has no short-lived credential, so it is rejected before
    /// any request. Maps to <c>getToken</c>.
    /// </summary>
    internal static async Task<OpenAIRealtimeClientSecret> GetTokenAsync(
        OpenAIProvider provider,
        string model,
        OpenAIRealtimeOptions? options,
        RealtimeModelV4SessionConfig? sessionConfig,
        int? expiresAfterSeconds,
        CancellationToken cancellationToken)
    {
        var created = CreateModel(provider, model, options);
        if (created is OpenAIRealtimeModelLive)
        {
            const string Functionality = "Short-lived OpenAI credentials for the Live API. Use server WebSocket setup via GetServerWebSocketConfigAsync() with a server-side API key instead.";
            throw new UnsupportedFunctionalityException(Functionality, "'" + Functionality + "' functionality not supported.");
        }

        return await ((OpenAIRealtimeModel)created).CreateClientSecretAsync(sessionConfig, expiresAfterSeconds, cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveRealtimeApi(string modelId, OpenAIRealtimeOptions? options)
    {
        var api = options?.Api;
        if (api != null)
        {
            if (api != "live" && api != "realtime")
            {
                throw new InvalidArgumentException("api", api, "OpenAI realtime api must be \"live\" or \"realtime\".");
            }

            return api;
        }

        return OpenAIModelCatalog.GetOpenAIRealtimeModelEntry(modelId).Api;
    }
}
