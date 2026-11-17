// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;
using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google;

/// <summary>Helpers for the Gemini Live realtime tests: strict event comparison like vitest <c>toEqual</c>.</summary>
internal static class GoogleRealtimeTestSupport
{
    /// <summary>The commit that added the upstream tests that are not in the catalog snapshot yet.</summary>
    public const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    public static IReadOnlyList<RealtimeModelV4ServerEvent> Parse(this Mapper mapper, string json)
    {
        return mapper.ParseServerEvent(GoogleUpstream.Element(json));
    }

    public static RealtimeModelV4ServerEvent ParseOne(this Mapper mapper, string json)
    {
        return Assert.Single(mapper.Parse(json));
    }

    /// <summary>Compares every field of the event. Fields that are not named must be unset, and <c>raw</c> must be the given JSON.</summary>
    public static void AssertEvent(
        RealtimeModelV4ServerEvent actual,
        string raw,
        string type,
        string? responseId = null,
        string? itemId = null,
        string? delta = null,
        string? callId = null,
        string? name = null,
        string? arguments = null,
        string? transcript = null,
        string? rawType = null,
        string? status = null
    )
    {
        Assert.Equal(type, actual.Type);
        Assert.Equal(responseId, actual.ResponseId);
        Assert.Equal(itemId, actual.ItemId);
        Assert.Equal(delta, actual.Delta);
        Assert.Equal(callId, actual.CallId);
        Assert.Equal(name, actual.Name);
        Assert.Equal(arguments, actual.Arguments);
        Assert.Equal(transcript, actual.Transcript);
        Assert.Equal(rawType, actual.RawType);
        Assert.Equal(status, actual.Status);
        Assert.Null(actual.Usage);
        Assert.NotNull(actual.Raw);
        GoogleUpstream.JsonEqual(actual.Raw!.Value, raw);
    }

    /// <summary>A realtime model built from the provider, as <c>provider.RealtimeModel</c> does.</summary>
    public static global::Vercel.AI.Google.GoogleRealtimeModel CreateModel(
        RecordingHandler handler,
        string baseUrl = "https://generativelanguage.googleapis.com/v1beta",
        string modelId = "gemini-2.0-flash-live-001"
    )
    {
        var provider = GoogleUpstream.Client(
            handler,
            new GoogleOptions { ApiKey = "test-key", BaseUrl = baseUrl }
        );
        return (global::Vercel.AI.Google.GoogleRealtimeModel)provider.RealtimeModel(modelId);
    }

    /// <summary>A realtime model built from a config, as <c>new GoogleRealtimeModel(modelId, config)</c> does.</summary>
    public static global::Vercel.AI.Google.GoogleRealtimeModel CreateModel(
        string modelId,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers,
        string baseUrl = "https://generativelanguage.googleapis.com/v1beta"
    )
    {
        return new global::Vercel.AI.Google.GoogleRealtimeModel(
            modelId,
            new GoogleRealtimeModelConfig("google.realtime", baseUrl, headers)
        );
    }

    /// <summary>Headers that carry the Gemini API key.</summary>
    public static Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> ApiKeyHeaders(
        params (string Name, string? Value)[] extra
    )
    {
        IReadOnlyDictionary<string, string?> headers = new Dictionary<string, string?>(
            extra.ToDictionary(pair => pair.Name, pair => pair.Value)
        )
        {
            ["x-goog-api-key"] = "test-key",
        };
        return _ => Task.FromResult(headers);
    }
}
