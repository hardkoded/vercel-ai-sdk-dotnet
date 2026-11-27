// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.Operations;
using Model = Vercel.AI.Alibaba.AlibabaVideoModel;

namespace Vercel.AI.Tests.AlibabaVideoModel;

internal sealed record AlibabaVideoCall(
    HttpMethod Method,
    string Url,
    string Body,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Port of the <c>createTestServer</c> call of <c>alibaba-video-model.test.ts</c>. Every test gets its own server, so no response has to be reset.</summary>
internal sealed class AlibabaVideoServer : HttpMessageHandler
{
    public AlibabaVideoServer()
    {
        Json(AlibabaVideoModelFixture.CreateUrl, AlibabaVideoModelFixture.CreateTaskResponse);
        Json(AlibabaVideoModelFixture.TaskUrl, AlibabaVideoModelFixture.SucceededTaskResponse);
        Json(
            AlibabaVideoModelFixture.TestBaseUrl + "/api/v1/tasks/abc%2F..%2F..%2Finternal",
            AlibabaVideoModelFixture.SucceededTaskResponse
        );
        Json(
            AlibabaVideoModelFixture.TestBaseUrl + "/api/v1/tasks/%252E",
            AlibabaVideoModelFixture.SucceededTaskResponse
        );
        Json(
            AlibabaVideoModelFixture.TestBaseUrl + "/api/v1/tasks/%252E%252E",
            AlibabaVideoModelFixture.SucceededTaskResponse
        );
    }

    public List<AlibabaVideoCall> Calls { get; } = [];

    public Dictionary<string, Func<HttpResponseMessage>> Urls { get; } =
        new(StringComparer.Ordinal);

    public void Json(string url, string body)
    {
        Urls[url] = () =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var body =
            request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        var url = request.RequestUri!.AbsoluteUri;
        lock (Calls)
        {
            Calls.Add(new AlibabaVideoCall(request.Method, url, body, headers));
        }

        return Urls.TryGetValue(request.RequestUri.GetLeftPart(UriPartial.Path), out var respond)
            ? respond()
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}

internal static class AlibabaVideoModelFixture
{
    public const string PromptText = "A serene mountain lake at sunset with gentle ripples";
    public const string TestBaseUrl = "https://dashscope-intl.aliyuncs.com";
    public const string CreateUrl =
        TestBaseUrl + "/api/v1/services/aigc/video-generation/video-synthesis";
    public const string TaskUrl = TestBaseUrl + "/api/v1/tasks/task-abc-123";

    public const string CreateTaskResponse =
        "{\"output\":{\"task_status\":\"PENDING\",\"task_id\":\"task-abc-123\"},\"request_id\":\"req-001\"}";

    public const string SucceededTaskResponse =
        "{\"output\":{\"task_id\":\"task-abc-123\",\"task_status\":\"SUCCEEDED\",\"video_url\":\"https://dashscope-result.oss.aliyuncs.com/output/video-001.mp4\",\"submit_time\":\"2024-01-01 00:00:00.000\",\"scheduled_time\":\"2024-01-01 00:00:01.000\",\"end_time\":\"2024-01-01 00:01:00.000\",\"orig_prompt\":\""
        + PromptText
        + "\",\"actual_prompt\":\"An enhanced prompt with more cinematic details\"},\"usage\":{\"duration\":5.0,\"output_video_duration\":5,\"SR\":1080,\"size\":\"1920x1080\"},\"request_id\":\"req-002\"}";

    /// <summary>The PNG magic bytes the upstream tests use.</summary>
    public static byte[] ImageData => [137, 80, 78, 71];

    /// <summary>Port of <c>createModel</c>.</summary>
    public static Model CreateModel(
        AlibabaVideoServer server,
        string modelId = "wan2.6-t2v",
        IReadOnlyDictionary<string, string?>? headers = null,
        Func<DateTime>? currentDate = null
    )
    {
        var configHeaders = headers
            ?? new Dictionary<string, string?> { ["Authorization"] = "Bearer test-api-key" };
        return new Model(
            modelId,
            new AlibabaVideoModelConfig(
                "alibaba.video",
                TestBaseUrl,
                _ => Task.FromResult(configHeaders),
                new HttpClient(server, disposeHandler: false)
            )
            {
                CurrentDate = currentDate,
            }
        );
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="alibaba"/> is the JSON of <c>providerOptions.alibaba</c>.</summary>
    public static VideoModelCall Call(
        string? prompt = PromptText,
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
        string? alibaba = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var providerOptions = alibaba == null ? "{}" : "{\"alibaba\":" + alibaba + "}";
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
            CancellationToken.None
        );
    }

    /// <summary>A status call. <paramref name="operationJson"/> is the JSON of the operation.</summary>
    public static VideoModelCall StatusCall(string operationJson)
    {
        return new VideoModelCall(
            null,
            1,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            JsonDocument.Parse("{}").RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None,
            operation: JsonDocument.Parse(operationJson).RootElement.Clone()
        );
    }

    public static Task<VideoStartResult> Start(Model model, VideoModelCall call)
    {
        return model.DoStartAsync(call, CancellationToken.None);
    }

    public static Task<VideoStatusResult> Status(Model model, string taskId)
    {
        return model.DoStatusAsync(
            StatusCall(new JsonObject { ["taskId"] = taskId }.ToJsonString()),
            CancellationToken.None
        );
    }

    public static VideoFrameFile Frame(string frameType, VideoModelFile image)
    {
        return new VideoFrameFile(image, frameType);
    }

    public static VideoModelFile Url(string url)
    {
        return VideoModelFile.FromUrl(url);
    }

    /// <summary>The JSON body of the first request, like <c>server.calls[0].requestBodyJson</c>.</summary>
    public static JsonNode RequestBody(AlibabaVideoServer server)
    {
        return JsonNode.Parse(server.Calls[0].Body)!;
    }

    /// <summary><c>toStrictEqual</c> for JSON.</summary>
    public static void AssertJson(string expected, JsonNode? actual)
    {
        var expectedNode = JsonNode.Parse(expected);
        Assert.True(
            JsonNode.DeepEquals(expectedNode, actual),
            "Expected " + expectedNode?.ToJsonString() + " but was " + actual?.ToJsonString()
        );
    }

    /// <summary><c>toMatchObject</c> for JSON: objects match by subset, arrays match item by item, other values must be equal.</summary>
    public static void AssertMatches(string expected, JsonNode? actual)
    {
        Assert.True(
            Matches(JsonNode.Parse(expected), actual),
            "Expected a match of " + expected + " but was " + actual?.ToJsonString()
        );
    }

    /// <summary><c>not.toHaveProperty</c>, or <c>toBeUndefined</c> for a property.</summary>
    public static void AssertNoProperty(string name, JsonNode? actual)
    {
        Assert.False(
            actual!.AsObject().ContainsKey(name),
            "Unexpected property " + name + " in " + actual.ToJsonString()
        );
    }

    public static JsonNode? Operation(VideoStartResult result)
    {
        return JsonNode.Parse(((JsonElement)result.Operation!).GetRawText());
    }

    public static JsonNode? Metadata(VideoStatusResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText());
    }

    /// <summary><c>expect.objectContaining({ type: 'unsupported', feature })</c>.</summary>
    public static bool HasWarning(IReadOnlyList<OperationWarning> warnings, string feature)
    {
        return warnings.Any(warning => warning.Type == "unsupported" && warning.Feature == feature);
    }

    /// <summary><c>expect.objectContaining({ feature })</c>.</summary>
    public static bool HasFeature(IReadOnlyList<OperationWarning> warnings, string feature)
    {
        return warnings.Any(warning => warning.Feature == feature);
    }

    private static bool Matches(JsonNode? expected, JsonNode? actual)
    {
        if (expected is JsonObject expectedObject)
        {
            return actual is JsonObject actualObject
                && expectedObject.All(pair =>
                    actualObject.TryGetPropertyValue(pair.Key, out var value)
                    && Matches(pair.Value, value)
                );
        }

        if (expected is JsonArray expectedArray)
        {
            return actual is JsonArray actualArray
                && expectedArray.Count == actualArray.Count
                && expectedArray
                    .Select((item, index) => Matches(item, actualArray[index]))
                    .All(match => match);
        }

        return JsonNode.DeepEquals(expected, actual);
    }
}
