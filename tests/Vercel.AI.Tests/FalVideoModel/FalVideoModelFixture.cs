// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Fal;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Model = Vercel.AI.Fal.FalVideoModel;

namespace Vercel.AI.Tests.FalVideoModel;

internal sealed record FalVideoCall(
    HttpMethod Method,
    string Url,
    string Body,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Port of the <c>createTestServer</c> call of <c>fal-video-model.test.ts</c>. Every test gets its own server, so no response has to be reset.</summary>
internal sealed class FalVideoServer : HttpMessageHandler
{
    public FalVideoServer()
    {
        Json(
            "https://queue.fal.run/fal-ai/luma-dream-machine",
            "{\"request_id\":\"test-request-id-123\",\"response_url\":\"https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-request-id-123\"}"
        );
        Json(
            FalVideoModelFixture.DreamMachineStatusUrl,
            "{\"video\":{\"url\":\"https://fal.media/files/video-output.mp4\",\"width\":1920,\"height\":1080,\"duration\":5.0,\"fps\":24,\"content_type\":\"video/mp4\"},\"seed\":12345,\"timings\":{\"inference\":45.5}}"
        );
        Json(
            "https://queue.fal.run/fal-ai/luma-ray-2",
            "{\"request_id\":\"ray2-request-id\",\"response_url\":\"https://queue.fal.run/fal-ai/luma-ray-2/requests/ray2-request-id\"}"
        );
        Json(
            "https://queue.fal.run/fal-ai/luma-ray-2/requests/ray2-request-id",
            "{\"video\":{\"url\":\"https://fal.media/files/ray2-video.mp4\",\"width\":1280,\"height\":720,\"duration\":3.0,\"fps\":30,\"content_type\":\"video/mp4\"},\"seed\":67890}"
        );
    }

    public List<FalVideoCall> Calls { get; } = [];

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

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key.ToLowerInvariant()] = string.Join(",", header.Value);
            }
        }

        lock (Calls)
        {
            Calls.Add(
                new FalVideoCall(request.Method, request.RequestUri!.AbsoluteUri, body, headers)
            );
        }

        return Urls.TryGetValue(request.RequestUri.GetLeftPart(UriPartial.Path), out var respond)
            ? respond()
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}

internal static class FalVideoModelFixture
{
    public const string PromptText = "A futuristic city with flying cars";

    public const string DreamMachineStatusUrl =
        "https://queue.fal.run/fal-ai/luma-dream-machine/requests/test-request-id-123";

    /// <summary>Port of <c>createBasicModel</c>.</summary>
    public static Model CreateBasicModel(
        FalVideoServer server,
        IReadOnlyDictionary<string, string>? headers = null,
        Func<DateTimeOffset>? currentDate = null,
        string modelId = "luma-dream-machine"
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-key" };
        foreach (var header in headers ?? new Dictionary<string, string>())
        {
            options.Headers[header.Key] = header.Value;
        }

        return new Model(FalProvider.Create(options, server), modelId, currentDate);
    }

    /// <summary>The <c>defaultOptions</c> with overrides. <paramref name="fal"/> is the JSON of <c>providerOptions.fal</c>.</summary>
    public static VideoModelCall Call(
        string? aspectRatio = null,
        double? duration = null,
        double? seed = null,
        VideoModelFile? image = null,
        string? fal = null,
        string? webhookUrl = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var providerOptions = fal == null ? "{}" : "{\"fal\":" + fal + "}";
        return new VideoModelCall(
            PromptText,
            1,
            aspectRatio,
            null,
            duration,
            null,
            seed,
            image,
            null,
            null,
            null,
            JsonDocument.Parse(providerOptions).RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None,
            webhookUrl: webhookUrl
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

    /// <summary>The JSON body of the first request, like <c>server.calls[0].requestBodyJson</c>.</summary>
    public static JsonNode RequestBody(FalVideoServer server)
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

    public static JsonNode? Operation(VideoStartResult result)
    {
        return JsonNode.Parse(((JsonElement)result.Operation!).GetRawText());
    }

    public static JsonNode? Metadata(VideoStatusResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText());
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

        return JsonNode.DeepEquals(expected, actual);
    }
}
