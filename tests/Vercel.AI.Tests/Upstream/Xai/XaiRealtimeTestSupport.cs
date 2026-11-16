// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai;

/// <summary>Builds the model that <c>createModel</c> builds in <c>xai-realtime-model.test.ts</c>.</summary>
internal static class XaiRealtimeTestSupport
{
    public static global::Vercel.AI.Xai.XaiRealtimeModel CreateModel(HttpClient? httpClient = null)
    {
        IReadOnlyDictionary<string, string?> headers = new Dictionary<string, string?>
        {
            ["authorization"] = "Bearer test-key",
        };
        return new global::Vercel.AI.Xai.XaiRealtimeModel(
            "grok-voice-latest",
            new XaiRealtimeModelConfig("xai.realtime", "https://api.x.ai/v1", _ =>
                Task.FromResult(headers)
            )
            {
                HttpClient = httpClient,
            }
        );
    }
}
