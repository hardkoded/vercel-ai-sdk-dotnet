// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using HeyGenProviderType = Vercel.AI.HeyGen.HeyGenProvider;
using HeyGenVideoModelType = Vercel.AI.HeyGen.HeyGenVideoModel;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenVideoModel;

/// <summary>Fixtures and helpers shared by the <c>heygen-video-model.test.ts</c> port.</summary>
internal static class HeyGenVideoModelSupport
{
    internal const string BaseUrl = "https://api.heygen.com";
    internal const string CreateUrl = BaseUrl + "/v3/models/videos";
    internal const string StatusUrl = CreateUrl + "/video-1";
    internal const string PromptText = "A paper boat floats down a quiet stream.";
    internal const string Completed = "{\"status\":\"completed\",\"video_url\":\"https://resource2.heygen.ai/video.mp4\",\"duration\":5,\"width\":1344,\"height\":768,\"aspect_ratio\":\"16:9\",\"seed\":0}";
    internal const string OperationText = "{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\"}";

    internal static readonly DateTimeOffset TestDate = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    internal static readonly VideoModelFile Image = VideoModelFile.FromUrl("https://example.com/image.jpg", "image/jpeg");
    internal static readonly VideoModelFile Video = VideoModelFile.FromUrl("https://example.com/video.mp4", "video/mp4");
    internal static readonly VideoModelFile Audio = VideoModelFile.FromUrl("https://example.com/audio.mp3", "audio/mpeg");

    /// <summary>A scripted HeyGen server. POST creates a video and GET reads its status.</summary>
    internal sealed class Server
    {
        internal Server()
        {
            Handler = new ParityHandler(call => call.Method == HttpMethod.Post ? Create(call) : Status(call));
        }

        internal ParityHandler Handler { get; }

        internal string CreateBody { get; set; } = "{\"data\":{\"status\":\"pending\",\"video_id\":\"video-1\"}}";

        internal string StatusBody { get; set; } = "{\"data\":" + Completed + "}";

        internal int CreateStatus { get; set; } = 200;

        internal int StatusStatus { get; set; } = 200;

        internal List<ParityCall> Calls => Handler.Calls;

        internal JsonObject RequestBody(int index = 0)
        {
            return JsonNode.Parse(Calls[index].Text)!.AsObject();
        }

        private HttpResponseMessage Create(ParityCall call) => Reply(CreateStatus, CreateBody);

        private HttpResponseMessage Status(ParityCall call) => Reply(StatusStatus, StatusBody);

        private static HttpResponseMessage Reply(int status, string body)
        {
            var response = ParityHandler.Json(body);
            response.StatusCode = (System.Net.HttpStatusCode)status;
            return response;
        }
    }

    internal static HeyGenVideoModelType CreateModel(HttpMessageHandler handler)
    {
        var provider = HeyGenProviderType.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, handler);
        return new HeyGenVideoModelType(provider, "heygen-video-1", () => TestDate);
    }

    /// <summary>The upstream <c>defaultOptions</c> with overrides.</summary>
    internal static VideoModelCall Start(
        string? prompt = PromptText,
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        int? fps = null,
        int? seed = null,
        VideoModelFile? image = null,
        IReadOnlyList<VideoFrameFile>? frames = null,
        IReadOnlyList<VideoModelFile>? references = null,
        bool? generateAudio = null,
        string heygen = "{}",
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return new VideoModelCall(prompt, n, aspectRatio, resolution, duration, fps, seed, image, frames, references, generateAudio, OperationJson_Parse("{\"heygen\":" + heygen + "}"), headers ?? new Dictionary<string, string>(), cancellationToken);
    }

    internal static VideoModelCall Status(string? operation = OperationText, CancellationToken cancellationToken = default)
    {
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson_Parse("{}"), new Dictionary<string, string>(), cancellationToken, operation: operation == null ? null : OperationJson_Parse(operation));
    }

    internal static IReadOnlyList<VideoFrameFile> Frames(params (VideoModelFile Image, string FrameType)[] frames)
    {
        return frames.Select(frame => new VideoFrameFile(frame.Image, frame.FrameType)).ToList();
    }

    internal static IReadOnlyList<VideoModelFile> Repeat(VideoModelFile file, int count)
    {
        return Enumerable.Repeat(file, count).ToList();
    }

    internal static JsonElement OperationJson_Parse(string json)
    {
        return Vercel.AI.Operations.OperationJson.Parse(json);
    }

    internal static JsonNode Node(JsonElement? element)
    {
        return JsonNode.Parse(element!.Value.GetRawText())!;
    }

    /// <summary>Like vitest <c>toMatchObject</c>: objects match when every expected property matches.</summary>
    internal static void Matches(JsonNode? actual, string expected)
    {
        Assert.True(IsMatch(actual, JsonNode.Parse(expected)), "Expected " + expected + " but was " + (actual?.ToJsonString() ?? "null"));
    }

    private static bool IsMatch(JsonNode? actual, JsonNode? expected)
    {
        switch (expected)
        {
            case JsonObject expectedObject:
                return actual is JsonObject actualObject
                    && expectedObject.All(pair => actualObject.ContainsKey(pair.Key) && IsMatch(actualObject[pair.Key], pair.Value));
            case JsonArray expectedArray:
                return actual is JsonArray actualArray
                    && actualArray.Count == expectedArray.Count
                    && expectedArray.Select((item, index) => IsMatch(actualArray[index], item)).All(match => match);
            default:
                return JsonNode.DeepEquals(actual, expected);
        }
    }
}
