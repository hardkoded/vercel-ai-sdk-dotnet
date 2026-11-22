// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Replicate;
using Model = Vercel.AI.Replicate.ReplicateVideoModel;

namespace Vercel.AI.Tests.ReplicateVideoModel;

internal sealed record ReplicateVideoCall(
    HttpMethod Method,
    string Url,
    string Body,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Port of the <c>fetch</c> mock of <c>createMockModel</c> and <c>createStatusModel</c> in <c>replicate-video-model.test.ts</c>.</summary>
internal sealed class ReplicateVideoServer(
    string predictionId,
    string predictionStatus,
    string? output,
    string? error,
    string metrics
) : HttpMessageHandler
{
    public List<ReplicateVideoCall> Calls { get; } = [];

    /// <summary>The request that created the prediction, like the <c>onRequest</c> filter of the upstream tests.</summary>
    public ReplicateVideoCall? CreateCall =>
        Calls.FirstOrDefault(call =>
            call.Url.Contains("/predictions", StringComparison.Ordinal)
            && !call.Url.Contains("test-prediction", StringComparison.Ordinal)
        );

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
            Calls.Add(new ReplicateVideoCall(request.Method, url, body, headers));
        }

        var isCreate =
            url.Contains("/predictions", StringComparison.Ordinal)
            && request.Method != HttpMethod.Get
            && !url.Contains(predictionId, StringComparison.Ordinal);
        var isStatus = url.Contains("/predictions/" + predictionId, StringComparison.Ordinal);
        if (!isCreate && !isStatus)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found"),
            };
        }

        var prediction = new JsonObject
        {
            ["id"] = predictionId,
            ["status"] = predictionStatus,
            ["output"] = output,
            ["error"] = error,
            ["urls"] = new JsonObject
            {
                ["get"] = "https://api.replicate.com/v1/predictions/" + predictionId,
            },
            ["metrics"] = JsonNode.Parse(metrics),
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                prediction.ToJsonString(),
                Encoding.UTF8,
                "application/json"
            ),
        };
    }
}

internal static class ReplicateVideoModelFixture
{
    public const string Prompt = "A rocket launching into space";
    public const string DefaultOutput = "https://replicate.delivery/video.mp4";
    public const string DefaultMetrics = "{\"predict_time\":25.5}";

    /// <summary>Port of <c>createMockModel</c> and <c>createStatusModel</c>.</summary>
    public static (Model Model, ReplicateVideoServer Server) CreateModel(
        string modelId = "minimax/video-01",
        Func<DateTimeOffset>? currentDate = null,
        string predictionId = "test-prediction-id",
        string predictionStatus = "succeeded",
        string? output = DefaultOutput,
        string? error = null,
        string metrics = DefaultMetrics
    )
    {
        var server = new ReplicateVideoServer(
            predictionId,
            predictionStatus,
            output,
            error,
            metrics
        );
        var options = new OpenAICompatibleOptions
        {
            BaseUrl = "https://api.replicate.com/v1",
            ApiKey = "test-api-token",
        };
        return (new Model(ReplicateProvider.Create(options, server), modelId, currentDate), server);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="replicate"/> is the JSON of <c>providerOptions.replicate</c>.</summary>
    public static VideoModelCall Call(
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        int? fps = null,
        double? seed = null,
        VideoModelFile? image = null,
        string? webhookUrl = null,
        string? replicate = null
    )
    {
        var providerOptions = replicate == null ? "{}" : "{\"replicate\":" + replicate + "}";
        return new VideoModelCall(
            Prompt,
            1,
            aspectRatio,
            resolution,
            duration,
            fps,
            seed,
            image,
            null,
            null,
            null,
            JsonDocument.Parse(providerOptions).RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None,
            webhookUrl
        );
    }

    /// <summary>A status call. <paramref name="getUrl"/> is the <c>getUrl</c> of the operation.</summary>
    public static VideoModelCall StatusCall(string getUrl)
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
            operation: JsonDocument
                .Parse(new JsonObject { ["getUrl"] = getUrl }.ToJsonString())
                .RootElement.Clone()
        );
    }

    /// <summary>The JSON body of the request that created the prediction.</summary>
    public static JsonNode CapturedBody(ReplicateVideoServer server)
    {
        return JsonNode.Parse(server.CreateCall!.Body)!;
    }

    /// <summary><c>toMatchObject</c> for JSON: objects match by subset, other values must be equal.</summary>
    public static void AssertMatches(string expected, JsonNode? actual)
    {
        Assert.True(
            Matches(JsonNode.Parse(expected), actual),
            "Expected a match of " + expected + " but was " + actual?.ToJsonString()
        );
    }

    public static JsonNode? Operation(VideoStartResult result)
    {
        return JsonNode.Parse(((JsonElement)result.Operation!).GetRawText());
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
