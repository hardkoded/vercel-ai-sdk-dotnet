// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GatewayImageModel;

internal static class GatewayImageModelFixture
{
    public const string ModelId = "openai/gpt-image-1";

    /// <summary>Mirrors <c>prepareJsonResponse()</c> and <c>createTestModel()</c> of the upstream test.</summary>
    public static async Task<UpstreamRecordingHandler> DoGenerateAsync(ImageCallOptions options)
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[\"AQID\"]}");
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler);
        await provider.ImageModel(ModelId).DoGenerateAsync(options, CancellationToken.None);
        return handler;
    }

    public static JsonObject RequestBody(UpstreamRecordingHandler handler) => JsonNode.Parse(handler.Body)!.AsObject();

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
