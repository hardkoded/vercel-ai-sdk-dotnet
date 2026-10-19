// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

internal static class GatewayImageModelSupport
{
    public const string ModelId = "openai/gpt-image-1";

    public static Vercel.AI.Gateway.GatewayImageModel CreateTestModel(HttpMessageHandler handler, string modelId = ModelId, IReadOnlyDictionary<string, string>? o11yHeaders = null)
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com", O11yHeaders = o11yHeaders ?? new Dictionary<string, string>() },
            handler);
        return (Vercel.AI.Gateway.GatewayImageModel)provider.ImageModel(modelId);
    }

    public static UpstreamRecordingHandler PrepareJsonResponse(string? images = null, string? extra = null)
    {
        return new UpstreamRecordingHandler("{\"images\":" + (images ?? "[\"base64-image-1\"]") + (extra == null ? string.Empty : "," + extra) + "}");
    }

    public static Task<ImageModelResult> DoGenerate(
        Vercel.AI.Gateway.GatewayImageModel model,
        string prompt = "Test prompt",
        IReadOnlyList<ImageModelFile>? files = null,
        ImageModelFile? mask = null,
        int n = 1,
        string? size = null,
        string? aspectRatio = null,
        int? seed = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var options = JsonDocument.Parse(providerOptions);
        return model.DoGenerateAsync(
            new ImageModelCall(prompt, files, mask, n, size, aspectRatio, seed, options.RootElement.Clone(), headers ?? new Dictionary<string, string>(), cancellationToken),
            cancellationToken);
    }

    public static JsonNode? Body(UpstreamRecordingHandler handler) => JsonNode.Parse(handler.Body);

    public static bool Equal(string expected, JsonNode? actual) => JsonNode.DeepEquals(JsonNode.Parse(expected), actual);
}
