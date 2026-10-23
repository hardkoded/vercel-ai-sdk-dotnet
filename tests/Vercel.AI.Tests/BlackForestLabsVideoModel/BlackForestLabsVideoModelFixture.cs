// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.BlackForestLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.BlackForestLabsImageModel;
using Model = Vercel.AI.BlackForestLabs.BlackForestLabsVideoModel;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

internal static class BlackForestLabsVideoModelFixture
{
    public const string PromptText = "A white kitten chases a butterfly across a sunlit garden.";
    public const string TestBaseUrl = "https://api.example.com/v1";
    public const string SubmitUrl = TestBaseUrl + "/flux-3-video";
    public const string PollUrl = "https://api.example.com/poll";
    public const string VideoUrl = "https://api.example.com/output/video-001.mp4";
    public const string RequestId = "req-123";

    public const string SubmitResponse = "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\",\"cost\":0.42,\"input_mp\":1.23,\"output_mp\":4.56}";

    public const string BareSubmitResponse = "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\"}";

    public const string ReadyResponse = "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/output/video-001.mp4\",\"seed\":7,\"duration\":8}}";

    public static readonly VideoModelFile ImageUrlFile = VideoModelFile.FromUrl("https://cdn.example.com/first.png", "image/png");

    public static readonly VideoModelFile LastImageUrlFile = VideoModelFile.FromUrl("https://cdn.example.com/last.png", "image/png");

    public static readonly VideoModelFile VideoUrlFile = VideoModelFile.FromUrl("https://cdn.example.com/clip.mp4", "video/mp4");

    /// <summary>The server of the upstream <c>createTestServer</c> call: a submit URL and a poll URL.</summary>
    public static BlackForestLabsServer CreateServer()
    {
        var server = new BlackForestLabsServer();
        server.Json(SubmitUrl, SubmitResponse);
        server.Json(PollUrl, ReadyResponse);
        return server;
    }

    /// <summary>Port of <c>createModel</c>.</summary>
    public static Model CreateModel(
        BlackForestLabsServer server,
        Func<DateTimeOffset>? currentDate = null,
        int pollIntervalMillis = 1,
        int pollTimeoutMillis = 5000)
    {
        var options = new OpenAICompatibleOptions { BaseUrl = TestBaseUrl, ApiKey = "test-key" };
        var provider = Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(options, server, pollIntervalMillis, pollTimeoutMillis);
        return new Model(provider, "flux-3-video", currentDate);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="blackForestLabs"/> is the JSON of <c>providerOptions.blackForestLabs</c>.</summary>
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
        string? blackForestLabs = null)
    {
        var providerOptions = blackForestLabs == null
            ? JsonDocument.Parse("{}").RootElement.Clone()
            : JsonDocument.Parse("{\"blackForestLabs\":" + blackForestLabs + "}").RootElement.Clone();
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
            providerOptions,
            new Dictionary<string, string>(),
            CancellationToken.None);
    }

    /// <summary>The options of a <c>doStatus</c> call. <paramref name="operationJson"/> is the operation.</summary>
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
            operation: JsonDocument.Parse(operationJson).RootElement.Clone());
    }

    /// <summary>The JSON body of the first request, like <c>server.calls[0].requestBodyJson</c>.</summary>
    public static JsonNode RequestBody(BlackForestLabsServer server)
    {
        return JsonNode.Parse(server.Calls[0].Body)!;
    }

    /// <summary><c>toStrictEqual</c> for JSON.</summary>
    public static void AssertJson(string expected, JsonNode? actual)
    {
        var expectedNode = JsonNode.Parse(expected);
        Assert.True(JsonNode.DeepEquals(expectedNode, actual), "Expected " + expectedNode?.ToJsonString() + " but was " + actual?.ToJsonString());
    }

    /// <summary><c>toMatchObject</c> for JSON: objects match by subset, other values must be equal.</summary>
    public static void AssertMatches(string expected, JsonNode? actual)
    {
        Assert.True(Matches(JsonNode.Parse(expected), actual), "Expected a match of " + expected + " but was " + actual?.ToJsonString());
    }

    public static void AssertNoProperty(string name, JsonNode? actual)
    {
        Assert.False(actual!.AsObject().ContainsKey(name), "Unexpected property " + name + " in " + actual.ToJsonString());
    }

    public static JsonNode? Videos(VideoModelResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["blackForestLabs"]!["videos"];
    }

    public static JsonNode? Videos(VideoStatusResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["blackForestLabs"]!["videos"];
    }

    public static JsonNode? Operation(VideoStartResult result)
    {
        return JsonNode.Parse(((JsonElement)result.Operation!).GetRawText());
    }

    public static (string Type, string? Feature, string? Details) Warning(OperationWarning warning)
    {
        return (warning.Type, warning.Feature, warning.Details);
    }

    public static HttpResponseMessage JsonResponse(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    private static bool Matches(JsonNode? expected, JsonNode? actual)
    {
        if (expected is JsonObject expectedObject)
        {
            return actual is JsonObject actualObject
                && expectedObject.All(pair => actualObject.TryGetPropertyValue(pair.Key, out var value) && Matches(pair.Value, value));
        }

        return JsonNode.DeepEquals(expected, actual);
    }
}
