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
using Model = Vercel.AI.BlackForestLabs.BlackForestLabsImageModel;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>One request seen by <see cref="BlackForestLabsServer"/>.</summary>
internal sealed class BlackForestLabsCall
{
    public BlackForestLabsCall(HttpMethod method, string url, string body, IReadOnlyDictionary<string, string> headers)
    {
        Method = method;
        Url = url;
        Body = body;
        Headers = headers;
    }

    public HttpMethod Method { get; }

    public string Url { get; }

    public string Body { get; }

    /// <summary>Request and content headers with lower-case names.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Headers without the SDK user agent and the computed content length.</summary>
    public Dictionary<string, string> HeadersWithoutUserAgent()
    {
        return Headers
            .Where(pair => pair.Key != "user-agent" && pair.Key != "content-length")
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public JsonNode? BodyJson => JsonNode.Parse(Body);
}

/// <summary>Test server that answers by URL, like <c>createTestServer</c>.</summary>
internal sealed class BlackForestLabsServer : HttpMessageHandler
{
    public BlackForestLabsServer()
    {
        const string submit = "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\"}";
        foreach (var model in new[] { "flux-2-pro", "flux-3-image", "flux-3-image-klein", "flux-pro-1.0-fill", "flux-kontext-pro" })
        {
            Json("https://api.example.com/v1/" + model, submit);
        }

        Json("https://api.example.com/poll", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\"}}");
        Binary("https://api.example.com/image.png");
        Binary("https://cdn.evil.example/image.png");
        Json("https://api.bfl.ai/v1/flux-2-pro", "{\"id\":\"req-123\",\"polling_url\":\"https://api.us1.bfl.ai/v1/get_result\"}");
        Json("https://api.us1.bfl.ai/v1/get_result", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://delivery-us1.bfl.ai/image.png\"}}");
        Binary("https://delivery-us1.bfl.ai/image.png");
        Binary("https://api.bfl.ai/image.png");
    }

    public List<BlackForestLabsCall> Calls { get; } = new List<BlackForestLabsCall>();

    public Dictionary<string, Func<CancellationToken, Task<HttpResponseMessage>>> Urls { get; } = new Dictionary<string, Func<CancellationToken, Task<HttpResponseMessage>>>();

    public void Json(string url, string body)
    {
        Respond(url, 200, body);
    }

    public void Respond(string url, int status, string body)
    {
        Urls[url] = _ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        });
    }

    public void Binary(string url)
    {
        Urls[url] = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("test-binary-content")),
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
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

        var url = request.RequestUri!.AbsoluteUri;
        lock (Calls)
        {
            Calls.Add(new BlackForestLabsCall(request.Method, url, body, headers));
        }

        var key = request.RequestUri.GetLeftPart(UriPartial.Path);
        if (!Urls.TryGetValue(key, out var respond))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        return await respond(cancellationToken).ConfigureAwait(false);
    }
}

internal static class BlackForestLabsImageModelFixture
{
    public const string PromptText = "A cute baby sea otter";

    /// <summary>Port of <c>createBasicModel</c>.</summary>
    public static Model CreateBasicModel(
        BlackForestLabsServer server,
        string modelId = "flux-2-pro",
        IReadOnlyDictionary<string, string>? headers = null,
        Func<DateTimeOffset>? currentDate = null,
        int? pollIntervalMillis = null,
        int? pollTimeoutMillis = null,
        string baseUrl = "https://api.example.com/v1")
    {
        var options = new OpenAICompatibleOptions { BaseUrl = baseUrl, ApiKey = "test-key" };
        if (headers != null)
        {
            foreach (var pair in headers)
            {
                options.Headers[pair.Key] = pair.Value;
            }
        }

        var provider = Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(options, server, pollIntervalMillis, pollTimeoutMillis);
        return new Model(provider, modelId, currentDate);
    }

    /// <summary>The <c>doGenerate</c> options. <paramref name="blackForestLabs"/> is the JSON of <c>providerOptions.blackForestLabs</c>.</summary>
    public static ImageModelCall Call(
        string? blackForestLabs = null,
        IReadOnlyList<ImageModelFile>? files = null,
        ImageModelFile? mask = null,
        string? size = null,
        string? aspectRatio = null,
        int? seed = null,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var providerOptions = blackForestLabs == null
            ? JsonDocument.Parse("{}").RootElement.Clone()
            : JsonDocument.Parse("{\"blackForestLabs\":" + blackForestLabs + "}").RootElement.Clone();
        return new ImageModelCall(
            PromptText,
            files,
            mask,
            1,
            size,
            aspectRatio,
            seed,
            providerOptions,
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None);
    }

    public static ImageModelFile PngFile(string content)
    {
        return ImageModelFile.FromFile(Encoding.UTF8.GetBytes(content), "image/png");
    }

    public static string Base64(string content)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
    }

    public static void AssertJson(string expected, string? actual)
    {
        var expectedNode = JsonNode.Parse(expected);
        var actualNode = JsonNode.Parse(actual ?? "null");
        Assert.True(JsonNode.DeepEquals(expectedNode, actualNode), "Expected " + expectedNode!.ToJsonString() + " but was " + actualNode?.ToJsonString());
    }

    public static JsonNode? Metadata(ImageModelResult result)
    {
        return JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["blackForestLabs"]!["images"]![0];
    }

    public static (string Type, string? Feature, string? Details) Warning(OperationWarning warning)
    {
        return (warning.Type, warning.Feature, warning.Details);
    }
}
