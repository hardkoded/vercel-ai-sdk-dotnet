// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.KlingAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using KlingAIVideoModelType = Vercel.AI.KlingAI.KlingAIVideoModel;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Fixtures and helpers shared by the <c>klingai-video-model.test.ts</c> port.</summary>
internal static class KlingAIVideoModelSupport
{
    internal const string TestBaseUrl = "https://api-singapore.klingai.com";
    internal const string Prompt = "A character performs a graceful dance";

    /// <summary>The upstream <c>createTaskResponse</c>.</summary>
    internal const string CreateTaskResponse = "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-001\",\"data\":{\"task_id\":\"task-abc-123\",\"task_status\":\"submitted\",\"task_info\":{\"external_task_id\":null},\"created_at\":1722769557708,\"updated_at\":1722769557708}}";

    /// <summary>The upstream <c>successfulTaskResponse</c>.</summary>
    internal const string SuccessfulTaskResponse = "{\"code\":0,\"message\":\"success\",\"request_id\":\"req-002\",\"data\":{\"task_id\":\"task-abc-123\",\"task_status\":\"succeed\",\"task_status_msg\":\"\",\"task_info\":{\"external_task_id\":null},\"watermark_info\":{\"enabled\":false},\"final_unit_deduction\":\"1\",\"created_at\":1722769557708,\"updated_at\":1722769560000,\"task_result\":{\"videos\":[{\"id\":\"video-001\",\"url\":\"https://p1.a.kwimgs.com/output/video-001.mp4\",\"watermark_url\":\"https://p1.a.kwimgs.com/output/video-001-watermark.mp4\",\"duration\":\"5.0\"}]}}}";

    /// <summary>The upstream <c>klingaiProviderOptions</c>: motion control.</summary>
    internal const string MotionControlOptions = "{\"klingai\":{\"videoUrl\":\"https://example.com/reference-motion.mp4\",\"characterOrientation\":\"image\",\"mode\":\"std\"}}";

    /// <summary>The upstream <c>t2vProviderOptions</c> and <c>i2vProviderOptions</c>.</summary>
    internal const string StdOptions = "{\"klingai\":{\"mode\":\"std\"}}";

    internal static readonly DateTimeOffset TestDate = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    internal static readonly VideoModelFile StartFrame = VideoModelFile.FromUrl("https://example.com/start-frame.png");

    /// <summary>A scripted KlingAI server. POST creates a task and GET reads it. Set a body per path to override.</summary>
    internal sealed class Server
    {
        private readonly Dictionary<string, (int Status, string Body)> _post = new(StringComparer.Ordinal);
        private readonly Dictionary<string, (int Status, string Body)> _get = new(StringComparer.Ordinal);

        internal Server()
        {
            Handler = new ParityHandler(call => call.Method == HttpMethod.Post ? Reply(_post, call, CreateTaskResponse) : Reply(_get, call, SuccessfulTaskResponse));
        }

        internal ParityHandler Handler { get; }

        internal List<ParityCall> Calls => Handler.Calls;

        /// <summary>Overrides the POST response for a path such as <c>/v1/videos/text2video</c>.</summary>
        internal void OnCreate(string path, string body, int status = 200)
        {
            _post[path] = (status, body);
        }

        /// <summary>Overrides the GET response for a path such as <c>/v1/videos/text2video/task-abc-123</c>.</summary>
        internal void OnStatus(string path, string body, int status = 200)
        {
            _get[path] = (status, body);
        }

        internal JsonObject RequestBody(int index = 0)
        {
            return JsonNode.Parse(Calls[index].Text)!.AsObject();
        }

        private static HttpResponseMessage Reply(Dictionary<string, (int Status, string Body)> routes, ParityCall call, string fallback)
        {
            var (status, body) = routes.TryGetValue(call.Uri.AbsolutePath, out var route) ? route : (200, fallback);
            var response = ParityHandler.Json(body);
            response.StatusCode = (System.Net.HttpStatusCode)status;
            return response;
        }
    }

    /// <summary>The upstream <c>createBasicModel</c>. The default Authorization header is <c>Bearer test-jwt-token</c>.</summary>
    internal static KlingAIVideoModelType CreateModel(
        HttpMessageHandler handler,
        string modelId = "kling-v2.6-motion-control",
        string baseUrl = TestBaseUrl,
        Func<DateTimeOffset>? currentDate = null,
        Action<OpenAICompatibleOptions>? configure = null)
    {
        var options = new KlingAIOptions { ApiKey = "test-jwt-token", BaseUrl = baseUrl };
        configure?.Invoke(options);
        var provider = Vercel.AI.KlingAI.KlingAIProvider.Create(options, handler);
        return new KlingAIVideoModelType(provider, modelId, currentDate);
    }

    /// <summary>The upstream <c>defaultOptions</c> (motion control) with overrides. The prompt, <c>n</c> and provider options are the upstream defaults.</summary>
    internal static VideoModelCall Start(
        string? prompt = Prompt,
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
        string providerOptions = MotionControlOptions,
        Dictionary<string, string>? headers = null,
        string? webhookUrl = null,
        CancellationToken cancellationToken = default)
    {
        return new VideoModelCall(prompt, n, aspectRatio, resolution, duration, fps, seed, image, frameImages, inputReferences, generateAudio, Parse(providerOptions), headers ?? new Dictionary<string, string>(), cancellationToken, webhookUrl);
    }

    /// <summary>The upstream <c>t2vDefaultOptions</c> with overrides.</summary>
    internal static VideoModelCall StartT2V(
        double? duration = null,
        string? aspectRatio = null,
        VideoModelFile? image = null,
        bool? generateAudio = null,
        string providerOptions = StdOptions,
        Dictionary<string, string>? headers = null,
        string? webhookUrl = null)
    {
        return Start(aspectRatio: aspectRatio, duration: duration, image: image, generateAudio: generateAudio, providerOptions: providerOptions, headers: headers, webhookUrl: webhookUrl);
    }

    /// <summary>The upstream <c>i2vDefaultOptions</c> with overrides. The default image is <see cref="StartFrame"/>.</summary>
    internal static VideoModelCall StartI2V(
        double? duration = null,
        string? aspectRatio = null,
        VideoModelFile? image = null,
        IReadOnlyList<VideoFrameFile>? frameImages = null,
        bool? generateAudio = null,
        string providerOptions = StdOptions,
        Dictionary<string, string>? headers = null,
        string? webhookUrl = null)
    {
        return Start(aspectRatio: aspectRatio, duration: duration, image: image ?? StartFrame, frameImages: frameImages, generateAudio: generateAudio, providerOptions: providerOptions, headers: headers, webhookUrl: webhookUrl);
    }

    /// <summary>A status call for <c>{ taskId, endpointPath }</c>.</summary>
    internal static VideoModelCall Status(string endpointPath = "/v1/videos/text2video", string taskId = "task-abc-123", Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var operation = "{\"taskId\":\"" + taskId + "\",\"endpointPath\":\"" + endpointPath + "\"}";
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, Parse("{}"), headers ?? new Dictionary<string, string>(), cancellationToken, operation: Parse(operation));
    }

    internal static IReadOnlyList<VideoFrameFile> Frames(params (VideoModelFile Image, string FrameType)[] frames)
    {
        return frames.Select(frame => new VideoFrameFile(frame.Image, frame.FrameType)).ToList();
    }

    internal static JsonElement Parse(string json)
    {
        return OperationJson.Parse(json);
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
