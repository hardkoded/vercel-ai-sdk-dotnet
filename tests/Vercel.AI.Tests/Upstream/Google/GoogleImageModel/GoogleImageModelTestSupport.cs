// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleImageModel;

/// <summary>Shared setup for <c>google-image-model.test.ts</c>.</summary>
internal static class GoogleImageModelTestSupport
{
    public const string TestUrl = "https://api.example.com/v1beta/models/gemini-2.5-flash-image:generateContent";

    public static GoogleProvider CreateProvider(RecordingHandler handler, Action<GoogleOptions>? configure = null)
    {
        var options = new GoogleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v1beta" };
        configure?.Invoke(options);
        return GoogleProvider.Create(options, handler);
    }

    public static Vercel.AI.Google.GoogleImageModel Model(RecordingHandler handler, string modelId = "gemini-2.5-flash-image", Action<GoogleOptions>? configure = null)
    {
        return new Vercel.AI.Google.GoogleImageModel(CreateProvider(handler, configure), modelId);
    }

    public static ImageModelCall Call(
        string prompt,
        IReadOnlyList<ImageModelFile>? files = null,
        ImageModelFile? mask = null,
        string? size = null,
        string? aspectRatio = null,
        int? seed = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null)
    {
        return new ImageModelCall(prompt, files, mask, 1, size, aspectRatio, seed, GoogleUpstream.Element(providerOptions), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }

    public static RecordingHandler Response(
        string images = "[{\"mimeType\":\"image/png\",\"data\":\"AQID\"}]",
        string usage = "{\"promptTokenCount\":10,\"candidatesTokenCount\":100,\"totalTokenCount\":110}",
        string finishReason = "STOP",
        string? groundingMetadata = null)
    {
        var parts = new JsonArray();
        foreach (var image in JsonNode.Parse(images)!.AsArray())
        {
            parts.Add(new JsonObject { ["inlineData"] = new JsonObject { ["mimeType"] = (string?)image!["mimeType"], ["data"] = (string?)image["data"] } });
        }

        var candidate = new JsonObject
        {
            ["content"] = new JsonObject { ["parts"] = parts, ["role"] = "model" },
            ["finishReason"] = finishReason,
        };
        if (groundingMetadata != null)
        {
            candidate["groundingMetadata"] = JsonNode.Parse(groundingMetadata);
        }

        var body = new JsonObject { ["candidates"] = new JsonArray(candidate), ["usageMetadata"] = JsonNode.Parse(usage) };
        return new RecordingHandler { ResponseText = body.ToJsonString() };
    }
}
