// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Model = Vercel.AI.Google.GoogleVideoModel;

namespace Vercel.AI.Tests.GoogleVideoModel;

internal sealed record GoogleVideoRequest(
    string Url,
    JsonNode? Body,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Port of the <c>fetch</c> mock of <c>createMockModel</c> in <c>google-video-model.test.ts</c>.</summary>
internal sealed class GoogleVideoFetch : HttpMessageHandler
{
    private readonly Func<string, HttpResponseMessage> _respond;

    public GoogleVideoFetch(Func<string, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public List<GoogleVideoRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var text =
            request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
        }

        var url = request.RequestUri!.AbsoluteUri;
        lock (Requests)
        {
            Requests.Add(
                new GoogleVideoRequest(
                    url,
                    string.IsNullOrEmpty(text) ? new JsonObject() : JsonNode.Parse(text),
                    headers
                )
            );
        }

        return _respond(url);
    }
}

internal static class GoogleVideoModelFixture
{
    public const string PromptText = "A futuristic city with flying cars";
    public const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    public const string DefaultVideoUri =
        "https://generativelanguage.googleapis.com/files/video-123.mp4";

    public static HttpResponseMessage Json(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    public static Model CreateModel(
        GoogleVideoFetch fetch,
        string modelId = "veo-3.1-generate-preview",
        Func<DateTimeOffset>? currentDate = null,
        string apiKey = "test-api-key"
    )
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = apiKey }, fetch);
        if (currentDate != null)
        {
            provider.Clock = currentDate;
        }

        return new Model(provider, modelId);
    }

    /// <summary>Port of <c>createMockModel</c>. <paramref name="videos"/> holds the sample URIs, or null for a sample without a video.</summary>
    public static (Model Model, GoogleVideoFetch Fetch) CreateMockModel(
        string modelId = "veo-3.1-generate-preview",
        Func<DateTimeOffset>? currentDate = null,
        string operationName = "operations/test-operation-id",
        bool operationDone = true,
        string? operationError = null,
        IReadOnlyList<string?>? videos = null,
        string apiKey = "test-api-key"
    )
    {
        videos ??= [DefaultVideoUri];
        var fetch = new GoogleVideoFetch(url =>
        {
            if (url.Contains(":predictLongRunning"))
            {
                return Json(
                    new JsonObject { ["name"] = operationName, ["done"] = false }.ToJsonString()
                );
            }

            if (url.Contains(operationName))
            {
                var operation = new JsonObject
                {
                    ["name"] = operationName,
                    ["done"] = operationDone,
                };
                if (operationError != null)
                {
                    operation["error"] = JsonNode.Parse(operationError);
                }
                else
                {
                    var samples = new JsonArray();
                    foreach (var uri in videos)
                    {
                        samples.Add(
                            new JsonObject
                            {
                                ["video"] =
                                    uri == null
                                        ? new JsonObject()
                                        : new JsonObject { ["uri"] = uri },
                            }
                        );
                    }

                    operation["response"] = new JsonObject
                    {
                        ["generateVideoResponse"] = new JsonObject
                        {
                            ["generatedSamples"] = samples,
                        },
                    };
                }

                return Json(operation.ToJsonString());
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found"),
            };
        });
        return (CreateModel(fetch, modelId, currentDate, apiKey), fetch);
    }

    /// <summary>A model whose every request gets the same JSON answer.</summary>
    public static (Model Model, GoogleVideoFetch Fetch) CreateFixedModel(
        string responseBody,
        Func<DateTimeOffset>? currentDate = null,
        string apiKey = "test-key"
    )
    {
        var fetch = new GoogleVideoFetch(_ => Json(responseBody));
        return (CreateModel(fetch, currentDate: currentDate, apiKey: apiKey), fetch);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="google"/> is the JSON of <c>providerOptions.google</c>.</summary>
    public static VideoModelCall Call(
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        double? seed = null,
        VideoModelFile? image = null,
        string? google = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var providerOptions = google == null ? "{}" : "{\"google\":" + google + "}";
        return new VideoModelCall(
            PromptText,
            n,
            aspectRatio,
            resolution,
            duration,
            null,
            seed,
            image,
            null,
            null,
            null,
            JsonDocument.Parse(providerOptions).RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    public static VideoModelCall StatusCall(
        string operationName,
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
            operation: JsonDocument
                .Parse("{\"operationName\":\"" + operationName + "\"}")
                .RootElement.Clone()
        );
    }

    /// <summary>The body of the <c>:predictLongRunning</c> request, like <c>capturedBody</c>.</summary>
    public static JsonNode? CapturedBody(GoogleVideoFetch fetch)
    {
        return fetch.Requests.First(request => request.Url.Contains(":predictLongRunning")).Body;
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
}
