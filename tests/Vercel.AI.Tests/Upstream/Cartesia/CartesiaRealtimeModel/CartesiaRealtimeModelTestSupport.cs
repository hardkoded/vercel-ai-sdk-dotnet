// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Cartesia;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaRealtimeModel;

/// <summary>Builds the model that <c>createModel</c> builds in <c>cartesia-realtime-model.test.ts</c>.</summary>
internal static class CartesiaRealtimeModelTestSupport
{
    public static global::Vercel.AI.Cartesia.CartesiaRealtimeModel CreateModel(
        HttpClient? httpClient = null
    )
    {
        IReadOnlyDictionary<string, string?> headers = new Dictionary<string, string?>
        {
            ["Authorization"] = "Bearer test-key",
            ["Cartesia-Version"] = "2026-03-01",
        };
        return new global::Vercel.AI.Cartesia.CartesiaRealtimeModel(
            "ink-2",
            new CartesiaRealtimeModelConfig(
                "cartesia.realtime",
                "https://api.cartesia.ai",
                "2026-03-01",
                _ => Task.FromResult(headers)
            )
            {
                HttpClient = httpClient,
                CurrentDate = () => new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc),
            }
        );
    }

    public static System.Text.Json.JsonElement Json(string json)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
