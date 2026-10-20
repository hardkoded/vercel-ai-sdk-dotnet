// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.MiniMax;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>One request seen by <see cref="MiniMaxServer"/>.</summary>
internal sealed record MiniMaxCall(string Url, string RawBody, Dictionary<string, string> Headers)
{
    public JsonNode Body => JsonNode.Parse(RawBody)!;
}

/// <summary>Stands in for <c>createTestServer</c>. Records every call and answers with the configured body.</summary>
internal sealed class MiniMaxServer : HttpMessageHandler
{
    public const string Url = "https://api.minimax.io/anthropic/v1/messages";

    private readonly object _gate = new object();
    private readonly List<MiniMaxCall> _calls = new List<MiniMaxCall>();

    public MiniMaxServer()
    {
        SetJson("{\"type\":\"message\",\"content\":[{\"type\":\"text\",\"text\":\"Hello!\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":3}}");
    }

    public string ResponseBody { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = "application/json";

    public IReadOnlyList<MiniMaxCall> Calls
    {
        get
        {
            lock (_gate)
            {
                return _calls.ToArray();
            }
        }
    }

    public void SetJson(string body)
    {
        ResponseBody = body;
        ContentType = "application/json";
    }

    public void SetStream(params string[] chunks)
    {
        ResponseBody = string.Concat(chunks);
        ContentType = "text/event-stream";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        lock (_gate)
        {
            _calls.Add(new MiniMaxCall(request.RequestUri!.AbsoluteUri, body, headers));
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, ContentType),
        };
    }
}

/// <summary>Shared fixtures for the MiniMax chat language model tests.</summary>
internal static class MiniMaxParity
{
    public const string StreamMessageStart = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_flash\",\"model\":\"minimax-m3.1-flash\",\"usage\":{\"input_tokens\":4,\"output_tokens\":0}}}\n\n";

    public const string StreamEnd =
        "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":0}}\n\n" +
        "data: {\"type\":\"message_stop\"}\n\n";

    public static global::Vercel.AI.MiniMax.MiniMaxProvider Provider(MiniMaxServer server)
    {
        return global::Vercel.AI.MiniMax.MiniMaxProvider.Create(new MiniMaxOptions { ApiKey = "test-api-key" }, server);
    }

    public static LanguageModelCallOptions Hello()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };
    }

    public static IReadOnlyDictionary<string, JsonElement> Options(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
    }

    public static IReadOnlyDictionary<string, JsonElement> Options(string provider, string json)
    {
        return Options("{\"" + provider + "\":" + json + "}");
    }

    public static FileContentPart File(string mediaType, byte[] data, string? options = null)
    {
        return new FileContentPart(mediaType, null, data, null) { ProviderOptions = options == null ? null : Options("minimax", options) };
    }

    public static FileContentPart FileUrl(string mediaType, string url, string? options = null)
    {
        return new FileContentPart(mediaType, url, null, null) { ProviderOptions = options == null ? null : Options("minimax", options) };
    }

    public static FileContentPart FileReference(string mediaType, string provider, string id, string? options = null)
    {
        return FileContentPart.FromProviderReference(mediaType, new Dictionary<string, string> { [provider] = id }, null, options == null ? null : Options("minimax", options));
    }

    public static LanguageModelCallOptions Media(params UserContentPart[] parts)
    {
        return new LanguageModelCallOptions { MaxOutputTokens = 1000, Prompt = new ModelMessage[] { new UserModelMessage(parts) } };
    }

    public static void AssertMatch(JsonNode? actual, string expected)
    {
        AssertMatch(actual, JsonNode.Parse(expected), "$");
    }

    public static async Task<List<LanguageModelStreamPart>> Collect(LanguageModelStreamResult result)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result.Stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    private static void AssertMatch(JsonNode? actual, JsonNode? expected, string path)
    {
        if (expected is JsonObject expectedObject)
        {
            var actualObject = Assert.IsType<JsonObject>(actual);
            foreach (var pair in expectedObject)
            {
                Assert.True(actualObject.ContainsKey(pair.Key), path + "." + pair.Key + " is missing in " + actualObject.ToJsonString());
                AssertMatch(actualObject[pair.Key], pair.Value, path + "." + pair.Key);
            }

            return;
        }

        if (expected is JsonArray expectedArray)
        {
            var actualArray = Assert.IsType<JsonArray>(actual);
            Assert.Equal(expectedArray.Count, actualArray.Count);
            for (var i = 0; i < expectedArray.Count; i++)
            {
                AssertMatch(actualArray[i], expectedArray[i], path + "[" + i + "]");
            }

            return;
        }

        Assert.True(JsonNode.DeepEquals(actual, expected), path + ": " + (actual?.ToJsonString() ?? "null") + " != " + (expected?.ToJsonString() ?? "null"));
    }
}
