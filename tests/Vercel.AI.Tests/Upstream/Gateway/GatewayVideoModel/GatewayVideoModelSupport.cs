// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

internal static class GatewayVideoModelSupport
{
    public const string ModelId = "google/veo-2.0-generate-001";

    public const string DefaultVideos = "[{\"type\":\"base64\",\"data\":\"base64-video-1\",\"mediaType\":\"video/mp4\"}]";

    public static Vercel.AI.Gateway.GatewayVideoModel CreateTestModel(HttpMessageHandler handler, string modelId = ModelId, IReadOnlyDictionary<string, string>? o11yHeaders = null, string providerName = "gateway")
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com", O11yHeaders = o11yHeaders ?? new Dictionary<string, string>() },
            handler);
        return new Vercel.AI.Gateway.GatewayVideoModel(provider, modelId, providerName);
    }

    /// <summary>Port of <c>prepareJsonResponse</c>: one SSE data event with a <c>result</c> payload.</summary>
    public static UpstreamRecordingHandler PrepareJsonResponse(string? videos = null, string? warnings = null, string? providerMetadata = null)
    {
        var payload = "{\"type\":\"result\",\"videos\":" + (videos ?? DefaultVideos)
            + (warnings == null ? string.Empty : ",\"warnings\":" + warnings)
            + (providerMetadata == null ? string.Empty : ",\"providerMetadata\":" + providerMetadata) + "}";
        return new UpstreamRecordingHandler("data: " + payload + "\n\n");
    }

    public static VideoModelCall Call(
        string prompt = "Test prompt",
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        int? fps = null,
        double? seed = null,
        VideoModelFile? image = null,
        IReadOnlyList<VideoFrameFile>? frameImages = null,
        IReadOnlyList<VideoModelFile>? inputReferences = null,
        bool? generateAudio = null,
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null,
        string? webhookUrl = null,
        string? operation = null,
        CancellationToken cancellationToken = default)
    {
        return new VideoModelCall(
            prompt,
            n,
            aspectRatio,
            resolution,
            duration,
            fps,
            seed,
            image,
            frameImages,
            inputReferences,
            generateAudio,
            JsonDocument.Parse(providerOptions).RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            cancellationToken,
            webhookUrl,
            operation == null ? null : JsonDocument.Parse(operation).RootElement.Clone());
    }

    public static Task<VideoModelResult> DoGenerate(Vercel.AI.Gateway.GatewayVideoModel model, VideoModelCall? call = null)
    {
        call ??= Call();
        return model.DoGenerateAsync(call, call.CancellationToken);
    }

    public static JsonNode? Body(UpstreamRecordingHandler handler) => JsonNode.Parse(handler.Body);

    public static bool Equal(string expected, JsonNode? actual) => JsonNode.DeepEquals(JsonNode.Parse(expected), actual);

    public static JsonNode? Node(JsonElement? element) => element == null ? null : JsonNode.Parse(element.Value.GetRawText());

    public static (string Type, string? Url, string? Data, string? MediaType) Video(VideoOutput video) =>
        (video.Type, video.Url, video.Data as string, video.MediaType);

    public static (string Type, string? Feature, string? Details, string? Message) Warning(OperationWarning warning) =>
        (warning.Type, warning.Feature, warning.Details, warning.Message);

    public static HttpStatusCode Ok => HttpStatusCode.OK;
}
