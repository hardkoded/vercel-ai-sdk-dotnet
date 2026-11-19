// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XaiSpeechModel;

/// <summary>Shared fixtures for the xAI speech tests. Mirrors the top of <c>xai-speech-model.test.ts</c>.</summary>
internal static class XaiSpeechTestSupport
{
    public const string Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c";
    public const string Url = "https://api.x.ai/v1/tts";
    public const string Text = "Hello from the AI SDK!";

    public static readonly byte[] Audio = [1, 2, 3, 4];

    public static Vercel.AI.Xai.XaiProvider CreateProvider(
        ParityHandler handler,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        configure?.Invoke(options);
        return Vercel.AI.Xai.XaiProvider.Create(options, handler);
    }

    /// <summary>Port of <c>prepareAudioResponse</c>.</summary>
    public static ParityHandler AudioHandler(
        string contentType = "audio/mpeg",
        params (string Name, string Value)[] headers
    )
    {
        return new ParityHandler(_ => ParityHandler.Bytes(Audio, contentType, headers));
    }

    /// <summary>Port of <c>prepareTimestampsResponse</c>. The audio is 4 bytes, base64 <c>AQIDBA==</c>.</summary>
    public static ParityHandler TimestampsHandler(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(_ =>
            ParityHandler.Json(
                "{\"audio\":\"AQIDBA==\",\"content_type\":\"audio/mpeg\",\"duration\":1.19,\"audio_timestamps\":{\"graph_chars\":[\"H\",\"i\"],\"graph_times\":[[0.04,0.06],[0.06,0.1]]}}",
                headers
            )
        );
    }

    public static ISpeechCaller Speech(Vercel.AI.Xai.XaiProvider provider) =>
        (ISpeechCaller)provider.SpeechModel();

    public static SpeechModelCall Call(
        string text = Text,
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string? xai = null,
        IReadOnlyDictionary<string, string>? headers = null
    ) =>
        new(
            text,
            voice,
            outputFormat,
            instructions,
            speed,
            language,
            JsonDocument.Parse(xai == null ? "{}" : "{\"xai\":" + xai + "}").RootElement.Clone(),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None
        );

    public static JsonNode Body(ParityCall call) => JsonNode.Parse(call.Text)!;

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

    public static JsonNode? Metadata(SpeechModelResult result) =>
        JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText());

    public static HttpResponseMessage Error(HttpStatusCode status, string body)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
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
