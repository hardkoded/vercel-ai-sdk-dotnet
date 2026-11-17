// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Xai;
using Model = Vercel.AI.Xai.XaiVideoModel;

namespace Vercel.AI.Tests.XaiVideoModel;

internal sealed record XaiVideoCall(
    HttpMethod Method,
    string Url,
    string Body,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Port of the <c>createTestServer</c> call of <c>xai-video-model.test.ts</c>. Every test gets its own server, so no response has to be reset.</summary>
internal sealed class XaiVideoServer : HttpMessageHandler
{
    public XaiVideoServer()
    {
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/generations",
            XaiVideoModelFixture.CreateVideoResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/edits",
            XaiVideoModelFixture.CreateVideoResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/extensions",
            XaiVideoModelFixture.CreateVideoResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/req-123",
            XaiVideoModelFixture.DoneStatusResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/abc%2F..%2F..%2Finternal",
            XaiVideoModelFixture.DoneStatusResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/%252E",
            XaiVideoModelFixture.DoneStatusResponse
        );
        Json(
            XaiVideoModelFixture.TestBaseUrl + "/videos/%252E%252E",
            XaiVideoModelFixture.DoneStatusResponse
        );
    }

    public List<XaiVideoCall> Calls { get; } = [];

    public Dictionary<string, Func<HttpResponseMessage>> Urls { get; } =
        new(StringComparer.Ordinal);

    public void Json(string url, string body)
    {
        Respond(url, 200, body);
    }

    public void Respond(string url, int status, string body)
    {
        Urls[url] = () =>
            new HttpResponseMessage((HttpStatusCode)status)
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
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        var url = request.RequestUri!.AbsoluteUri;
        lock (Calls)
        {
            Calls.Add(new XaiVideoCall(request.Method, url, body, headers));
        }

        return Urls.TryGetValue(request.RequestUri.GetLeftPart(UriPartial.Path), out var respond)
            ? respond()
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}

internal static class XaiVideoModelFixture
{
    public const string PromptText = "A chicken flying into the sunset";
    public const string TestBaseUrl = "https://api.example.com";
    public const string CreateVideoResponse = "{\"request_id\":\"req-123\"}";

    public const string DoneStatusResponse =
        "{\"status\":\"done\",\"video\":{\"url\":\"https://vidgen.x.ai/output/video-001.mp4\",\"duration\":5,\"respect_moderation\":true},\"model\":\"grok-imagine-video\",\"progress\":100}";

    public const string Req123Url = TestBaseUrl + "/videos/req-123";

    /// <summary>Port of <c>createModel</c>.</summary>
    public static Model CreateModel(
        XaiVideoServer server,
        IReadOnlyDictionary<string, string>? headers = null,
        Func<DateTimeOffset>? currentDate = null,
        string modelId = "grok-imagine-video"
    )
    {
        var options = new OpenAICompatibleOptions { BaseUrl = TestBaseUrl, ApiKey = "test-key" };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return new Model(XaiProvider.Create(options, server), modelId, currentDate);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="xai"/> is the JSON of <c>providerOptions.xai</c>.</summary>
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
        string? xai = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var providerOptions = xai == null ? "{}" : "{\"xai\":" + xai + "}";
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
    public static VideoModelCall StatusCall(
        string operationJson,
        IReadOnlyDictionary<string, string>? headers = null
    )
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
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            operation: JsonDocument.Parse(operationJson).RootElement.Clone()
        );
    }

    public static string Req123(bool hasUploadUrl = false)
    {
        return hasUploadUrl
            ? "{\"requestId\":\"req-123\",\"hasUploadUrl\":true}"
            : "{\"requestId\":\"req-123\"}";
    }

    public static VideoFrameFile Frame(string frameType, VideoModelFile image)
    {
        return new VideoFrameFile(image, frameType);
    }

    /// <summary>The JSON body of the first request, like <c>server.calls[0].requestBodyJson</c>.</summary>
    public static JsonNode RequestBody(XaiVideoServer server)
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

    /// <summary><c>toMatchObject</c> for JSON: objects match by subset, other values must be equal.</summary>
    public static void AssertMatches(string expected, JsonNode? actual)
    {
        Assert.True(
            Matches(JsonNode.Parse(expected), actual),
            "Expected a match of " + expected + " but was " + actual?.ToJsonString()
        );
    }

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

    public static (string Type, string? Feature, string? Details) Warning(OperationWarning warning)
    {
        return (warning.Type, warning.Feature, warning.Details);
    }

    /// <summary><c>expect.objectContaining({ type: 'unsupported', feature })</c>.</summary>
    public static bool HasWarning(IReadOnlyList<OperationWarning> warnings, string feature)
    {
        return warnings.Any(warning => warning.Type == "unsupported" && warning.Feature == feature);
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
