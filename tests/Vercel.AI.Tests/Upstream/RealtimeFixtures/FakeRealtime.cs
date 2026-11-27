// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.RealtimeFixtures;

/// <summary>Port of <c>__fixtures__/fake-realtime.ts</c> <c>liveModel</c>.</summary>
public sealed class FakeRealtimeModel : IClientRealtimeModel
{
    public string Provider => "test";

    public string ModelId => "live";

    public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> ParseServerEventHandler { get; set; } =
        raw => [EventFromRaw(raw)];

    public Func<Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>>>? CreateServerEventParserHandler { get; set; }

    public Func<RealtimeModelV4ClientEvent, Task<JsonNode?>> SerializeClientEventHandler { get; set; } =
        clientEvent => Task.FromResult<JsonNode?>(new JsonObject { ["type"] = clientEvent.Type });

    public Uri BuildUri() => new("wss://example.test");

    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw) =>
        ParseServerEventHandler(raw);

    public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>>? CreateServerEventParser() =>
        CreateServerEventParserHandler?.Invoke();

    public Task<JsonNode?> SerializeClientEventAsync(
        RealtimeModelV4ClientEvent clientEvent,
        RealtimeModelV4SessionIntent? intent = null
    ) => SerializeClientEventHandler(clientEvent);

    public JsonNode? GetHealthCheckResponse(JsonElement raw) => null;

    /// <summary>The identity parser of the fixture: <c>event =&gt; event as RealtimeServerEvent</c>.</summary>
    public static RealtimeModelV4ServerEvent EventFromRaw(JsonElement raw)
    {
        var serverEvent = new RealtimeModelV4ServerEvent(raw.GetProperty("type").GetString()!)
        {
            Raw = raw,
        };
        if (raw.TryGetProperty("usage", out var usage))
        {
            serverEvent.Usage = new RealtimeModelV4Usage
            {
                Seconds = usage.GetProperty("seconds").GetDouble(),
            };
        }

        return serverEvent;
    }
}

/// <summary>Port of the <c>flushEvents</c> and <c>deferred</c> helpers of <c>__fixtures__/fake-realtime.ts</c>.</summary>
public static class FakeRealtime
{
    public static Task FlushEvents() => Task.Delay(100);

    public static TaskCompletionSource<T> Deferred<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
