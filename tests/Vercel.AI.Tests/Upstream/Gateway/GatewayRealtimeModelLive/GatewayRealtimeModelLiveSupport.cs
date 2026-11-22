// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayRealtimeModelLive;

/// <summary>Fixtures shared by the <c>gateway-realtime-model-live.test.ts</c> port.</summary>
internal static class GatewayRealtimeModelLiveSupport
{
    internal const string UpstreamCommit = "7e55599fc74f33525233d62cc9153cac38352d5a";

    internal const string LiveUrl =
        "wss://ai-gateway.vercel.sh/v4/ai/live-model?ai-model-id=openai%2Fgpt-live-1.1";

    internal static string FilePrefix(string describe) =>
        "packages/gateway/src/gateway-realtime-model-live.test.ts::" + describe + "::";

    /// <summary>Port of the test file's <c>createTestModel</c>.</summary>
    internal static Vercel.AI.Gateway.GatewayRealtimeModelLive CreateTestModel(
        string modelId = "openai/gpt-live-1.1",
        string baseUrl = "https://ai-gateway.vercel.sh/v4/ai",
        string? teamIdOrSlug = null,
        Func<
            string,
            int?,
            string,
            CancellationToken,
            Task<GatewayMintedClientSecret>
        >? createClientSecret = null
    )
    {
        return new Vercel.AI.Gateway.GatewayRealtimeModelLive(
            modelId,
            new GatewayRealtimeModelLiveConfig(
                "gateway.live",
                baseUrl,
                _ =>
                    Task.FromResult<IReadOnlyDictionary<string, string?>>(
                        new Dictionary<string, string?>
                        {
                            ["Authorization"] = "Bearer vck_test-token",
                            ["x-unset"] = null,
                        }
                    ),
                createClientSecret
                    ?? (
                        (_, _, _, _) =>
                            Task.FromResult(new GatewayMintedClientSecret("vcst_test-secret"))
                    ),
                teamIdOrSlug
            )
        );
    }

    /// <summary>Builds a session config whose provider options are the parsed JSON.</summary>
    internal static RealtimeModelV4SessionConfig WithProviderOptions(string json) =>
        new() { ProviderOptions = JsonNode.Parse(json)!.AsObject() };

    internal static string Message(string role, string type) =>
        "{\"type\":\"message\",\"role\":\""
        + role
        + "\",\"content\":[{\"type\":\""
        + type
        + "\",\"text\":\"Hi\"}]}";

    internal static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), actual),
            "Expected " + expected + " but was " + (actual?.ToJsonString() ?? "null")
        );
    }

    internal static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
