// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Model = Vercel.AI.Google.GoogleVertexVideoModel;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

internal sealed record VertexVideoCall(HttpMethod Method, string Url, JsonNode? Body);

/// <summary>Port of the <c>fetch</c> mock of <c>createMockModel</c> in <c>google-vertex-video-model.test.ts</c>.</summary>
internal sealed class VertexVideoServer : HttpMessageHandler
{
    private int _pollCount;

    public string OperationName { get; init; } = "operations/test-operation-id";

    public bool OperationDone { get; init; } = true;

    /// <summary>JSON of <c>operationError</c>, or null.</summary>
    public string? OperationError { get; init; }

    /// <summary>JSON array of the <c>video</c> objects.</summary>
    public string Videos { get; init; } =
        "[{\"bytesBase64Encoded\":\"base64-video-data\",\"mimeType\":\"video/mp4\"}]";

    public int PollsUntilDone { get; init; } = 1;

    /// <summary>Replaces the whole response body of every request (the custom <c>fetch</c> of a test).</summary>
    public Func<string, string>? Respond { get; init; }

    public List<VertexVideoCall> Calls { get; } = [];

    public VertexVideoCall? Last(string method)
    {
        return Calls.LastOrDefault(call => call.Url.Contains(method));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var text =
            request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var url = request.RequestUri!.AbsoluteUri;
        lock (Calls)
        {
            Calls.Add(
                new VertexVideoCall(
                    request.Method,
                    url,
                    text.Length == 0 ? null : JsonNode.Parse(text)
                )
            );
        }

        string? body = Respond?.Invoke(url);
        if (body == null && url.Contains(":predictLongRunning"))
        {
            body = new JsonObject { ["name"] = OperationName, ["done"] = false }.ToJsonString();
        }
        else if (body == null && url.Contains(":fetchPredictOperation"))
        {
            var count = Interlocked.Increment(ref _pollCount);
            if (count < PollsUntilDone)
            {
                body = new JsonObject { ["name"] = OperationName, ["done"] = false }.ToJsonString();
            }
            else
            {
                var operation = new JsonObject
                {
                    ["name"] = OperationName,
                    ["done"] = OperationDone,
                };
                if (OperationError != null)
                {
                    operation["error"] = JsonNode.Parse(OperationError);
                }
                else
                {
                    operation["response"] = new JsonObject { ["videos"] = JsonNode.Parse(Videos) };
                }

                body = operation.ToJsonString();
            }
        }

        return body == null
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found"),
            }
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
    }
}

internal static class GoogleVertexVideoModelFixture
{
    public const string Prompt = "A futuristic city with flying cars";

    /// <summary>Port of <c>createMockModel</c>.</summary>
    public static Model CreateModel(
        VertexVideoServer server,
        Func<DateTimeOffset>? currentDate = null,
        string modelId = "veo-2.0-generate-001"
    )
    {
        var provider = GoogleVertexProvider.Create(
            new VertexOptions { BaseUrl = "https://api.example.com", ApiKey = "test-key" },
            server
        );
        return new Model(provider, modelId, currentDate);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="providerOptions"/> is the JSON of <c>providerOptions</c>.</summary>
    public static VideoModelCall Call(
        int n = 1,
        string? aspectRatio = null,
        string? resolution = null,
        double? duration = null,
        double? seed = null,
        VideoModelFile? image = null,
        string providerOptions = "{}"
    )
    {
        return new VideoModelCall(
            Prompt,
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
            new Dictionary<string, string>(),
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

    public static string Operation(string name)
    {
        return "{\"operationName\":\"" + name + "\"}";
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

    /// <summary>The body of the last <c>predictLongRunning</c> request, like <c>capturedBody</c>.</summary>
    public static JsonNode? CapturedBody(VertexVideoServer server)
    {
        return server.Last(":predictLongRunning")?.Body;
    }

    public static JsonNode? OperationOf(VideoStartResult result)
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
