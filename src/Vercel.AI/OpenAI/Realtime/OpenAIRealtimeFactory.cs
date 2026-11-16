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

        // The catalog entry of a known Live model selects the Live API; every other id uses the Realtime API.
        return modelId == "gpt-live-1" ? "live" : "realtime";
    }
}
