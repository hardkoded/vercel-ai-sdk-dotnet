// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Builds a Converse prompt-cache checkpoint.</summary>
public static class AmazonBedrockCachePoint
{
    /// <summary>
    /// Creates a cache point. A null <paramref name="ttl"/> uses the model default (five minutes)
    /// and omits the <c>ttl</c> field. Supported values are <c>5m</c> and <c>1h</c>.
    /// </summary>
    public static JsonObject Create(string? ttl = null)
    {
        var point = new JsonObject
        {
            ["type"] = "default",
        };
        if (!string.IsNullOrEmpty(ttl))
        {
            point["ttl"] = ttl;
        }

        return new JsonObject
        {
            ["cachePoint"] = point,
        };
    }
}
