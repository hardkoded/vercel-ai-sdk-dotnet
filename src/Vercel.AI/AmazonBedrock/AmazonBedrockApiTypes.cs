// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Port of <c>amazon-bedrock-api-types.ts</c>.</summary>
public static class AmazonBedrockApiTypes
{
    /// <summary>
    /// Creates a cache point block with an optional TTL.
    /// </summary>
    /// <param name="ttl">Cache TTL, <c>5m</c> or <c>1h</c>. When null, Bedrock uses its default 5-minute TTL.</param>
    public static JsonObject CreateAmazonBedrockCachePoint(string? ttl = null)
    {
        var cachePoint = new JsonObject { ["type"] = "default" };
        if (ttl != null)
        {
            cachePoint["ttl"] = ttl;
        }

        return new JsonObject { ["cachePoint"] = cachePoint };
    }
}
