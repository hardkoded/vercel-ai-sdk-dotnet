// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Cerebras;

/// <summary>Exposes the service tier Cerebras applied as <c>providerMetadata.cerebras.serviceTier</c>.</summary>
public sealed class CerebrasMetadataExtractor : OpenAICompatibleMetadataExtractor
{
    /// <summary>The shared instance.</summary>
    public static readonly CerebrasMetadataExtractor Instance = new();

    /// <inheritdoc />
    public override JsonObject? ExtractMetadata(JsonElement parsedBody)
    {
        return Build(GetServiceTier(parsedBody));
    }

    /// <inheritdoc />
    public override OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor()
    {
        return new StreamExtractor();
    }

    private static string? GetServiceTier(JsonElement value)
    {
        // The response tier may be absent or null when service-tier handling is not applied.
        return value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("service_tier", out var tier)
            && tier.ValueKind == JsonValueKind.String
                ? tier.GetString()
                : null;
    }

    private static JsonObject? Build(string? serviceTier)
    {
        return serviceTier != null
            ? new JsonObject { [CerebrasProvider.ProviderId] = new JsonObject { ["serviceTier"] = serviceTier } }
            : null;
    }

    private sealed class StreamExtractor : OpenAICompatibleStreamMetadataExtractor
    {
        private string? _serviceTier;

        public override void ProcessChunk(JsonElement parsedChunk)
        {
            _serviceTier = GetServiceTier(parsedChunk) ?? _serviceTier;
        }

        public override JsonObject? BuildMetadata()
        {
            return Build(_serviceTier);
        }
    }
}
