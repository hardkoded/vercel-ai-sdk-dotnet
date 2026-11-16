// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Live = Vercel.AI.OpenAI.OpenAIRealtimeModelLive;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAILiveEventMapper;

/// <summary>Port of <c>openai-live-event-mapper.test.ts</c> &gt; <c>OpenAI Live server events</c>.</summary>
public sealed class OpenAILiveServerEventsTests
{
    private const string Prefix = "packages/openai/src/live/openai-live-event-mapper.test.ts::OpenAI Live server events::";

    private static readonly Live s_model = (Live)OpenAIUpstream.Provider(new OpenAICapture()).RealtimeModel("gpt-live-1");

    /// <summary>The fields an event sets, by their upstream names, like <c>toMatchObject</c> sees them.</summary>
    private static JsonObject Project(RealtimeModelV4ServerEvent @event)
    {
        var fields = new JsonObject { ["type"] = @event.Type };
        Set(fields, "sessionId", @event.SessionId);
        Set(fields, "delegationMode", @event.DelegationMode);
        Set(fields, "reason", @event.Reason);
        Set(fields, "delta", @event.Delta);
        Set(fields, "speaker", @event.Speaker);
        Set(fields, "delegationId", @event.DelegationId);
        Set(fields, "target", @event.Target);
        Set(fields, "command", @event.Command);
        Set(fields, "clientEventId", @event.ClientEventId);
        Set(fields, "message", @event.Message);
        Set(fields, "code", @event.Code);
        Set(fields, "rawType", @event.RawType);
        Set(fields, "responseId", @event.ResponseId);
        Set(fields, "startMs", @event.StartMs);
        Set(fields, "endMs", @event.EndMs);
        Set(fields, "offsetMs", @event.OffsetMs);
        Set(fields, "contextWindowUsageRatio", @event.ContextWindowUsageRatio);
        if (@event.Usage != null)
        {
            fields["usage"] = RealtimeMapperUsage(@event.Usage);
        }

        return fields;
    }

    private static JsonObject RealtimeMapperUsage(RealtimeModelV4Usage usage)
    {
        return OpenAIRealtimeEventMapper.RealtimeMapperSupport.UsageFields(usage);
    }

    private static void Set(JsonObject target, string name, string? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    private static void Set(JsonObject target, string name, double? value)
    {
        if (value != null)
        {
            target[name] = value;
        }
    }

    /// <summary>True when every field of <paramref name="expected"/> is in <paramref name="actual"/>, like <c>toMatchObject</c>.</summary>
    private static void AssertMatches(JsonObject expected, JsonObject actual)
    {
        foreach (var pair in expected)
        {
            Assert.True(actual.TryGetPropertyValue(pair.Key, out var value), "missing " + pair.Key);
            if (pair.Value is JsonObject nested)
            {
                AssertMatches(nested, (JsonObject)value!);
            }
            else
            {
                Assert.Equal(pair.Value!.ToJsonString(), value!.ToJsonString());
            }
        }
    }

    private static void AssertRaw(string rawJson, RealtimeModelV4ServerEvent @event)
    {
        Assert.NotNull(@event.Raw);
        Assert.Equal(rawJson, @event.Raw!.Value.GetRawText());
    }

    [Theory]
    [InlineData("{\"type\":\"session.started\",\"session\":{\"id\":\"session-1\",\"model\":\"gpt-live-1\"}}", "{\"type\":\"session-started\",\"sessionId\":\"session-1\"}")]
    [InlineData("{\"type\":\"session.closed\",\"session\":{\"id\":\"session-1\"},\"usage\":{\"seconds\":12},\"reason\":\"close_requested\"}", "{\"type\":\"session-closed\",\"sessionId\":\"session-1\",\"usage\":{\"seconds\":12},\"reason\":\"close_requested\"}")]
    [InlineData("{\"type\":\"session.closed\",\"usage\":{\"seconds\":0},\"reason\":\"connection_lost\"}", "{\"type\":\"session-closed\",\"usage\":{\"seconds\":0},\"reason\":\"connection_lost\"}")]
    [InlineData("{\"type\":\"session.usage.updated\",\"usage\":{\"seconds\":12},\"context_window\":{\"usage_ratio\":0.42}}", "{\"type\":\"session-usage\",\"usage\":{\"seconds\":12},\"contextWindowUsageRatio\":0.42}")]
    [InlineData("{\"type\":\"session.output_audio.delta\",\"delta\":\"AAAA\"}", "{\"type\":\"audio-chunk\",\"delta\":\"AAAA\"}")]
    [InlineData("{\"type\":\"session.input_transcript.delta\",\"delta\":\" hello\",\"start_ms\":100,\"end_ms\":200}", "{\"type\":\"transcript-fragment\",\"speaker\":\"user\",\"delta\":\" hello\",\"startMs\":100,\"endMs\":200}")]
    [InlineData("{\"type\":\"session.output_transcript.delta\",\"delta\":\"Hi \",\"start_ms\":120,\"end_ms\":210}", "{\"type\":\"transcript-fragment\",\"speaker\":\"assistant\",\"delta\":\"Hi \",\"startMs\":120,\"endMs\":210}")]
    [InlineData("{\"type\":\"session.delegation.created\",\"offset_ms\":1000,\"delegation\":{\"id\":\"opaque-delegation\",\"target\":\"client\",\"type\":\"delegation\"}}", "{\"type\":\"delegation-created\",\"delegationId\":\"opaque-delegation\",\"target\":\"client\",\"offsetMs\":1000}")]
    [InlineData("{\"type\":\"session.input_audio.muted\",\"client_event_id\":\"mute-1\"}", "{\"type\":\"command-acknowledged\",\"command\":\"session.input_audio.mute\",\"clientEventId\":\"mute-1\"}")]
    [InlineData("{\"type\":\"session.input_audio.unmuted\"}", "{\"type\":\"command-acknowledged\",\"command\":\"session.input_audio.unmute\"}")]
    [InlineData("{\"type\":\"session.updated\",\"session\":{\"id\":\"session-1\"},\"client_event_id\":\"update-1\"}", "{\"type\":\"command-acknowledged\",\"command\":\"session.update\",\"clientEventId\":\"update-1\"}")]
    [InlineData("{\"type\":\"session.instructions.appended\",\"client_event_id\":\"append-1\",\"start_ms\":10,\"end_ms\":20}", "{\"type\":\"command-acknowledged\",\"command\":\"session.instructions.append\",\"clientEventId\":\"append-1\"}")]
    [InlineData("{\"type\":\"session.thinking.appended\",\"start_ms\":10,\"end_ms\":20}", "{\"type\":\"command-acknowledged\",\"command\":\"session.thinking.append\"}")]
    [InlineData("{\"type\":\"session.commentary.appended\",\"start_ms\":10,\"end_ms\":20}", "{\"type\":\"command-acknowledged\",\"command\":\"session.commentary.append\"}")]
    [InlineData("{\"type\":\"error\",\"error\":{\"message\":\"Rejected\",\"code\":\"immutable_field_update\",\"client_event_id\":\"update-1\"}}", "{\"type\":\"error\",\"message\":\"Rejected\",\"code\":\"immutable_field_update\",\"clientEventId\":\"update-1\"}")]
    [UpstreamTest(Prefix + "normalizes %j and retains the original event", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_and_retains_the_original_event(string rawJson, string expectedJson)
    {
        var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson));

        var @event = Assert.Single(parsed);
        AssertMatches((JsonObject)JsonNode.Parse(expectedJson)!, Project(@event));
        AssertRaw(rawJson, @event);
        Assert.Null(@event.ItemId);
        Assert.Null(@event.ResponseId);
    }

    [Fact]
    [UpstreamTest(Prefix + "retains cumulative usage snapshots rather than summing updates", Coverage = UpstreamCoverage.Covered)]
    public void Retains_cumulative_usage_snapshots_rather_than_summing_updates()
    {
        foreach (var seconds in new[] { 1, 2, 2, 3 })
        {
            var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json("{\"type\":\"session.usage.updated\",\"usage\":{\"seconds\":" + seconds + "}}"));

            var @event = Assert.Single(parsed);
            Assert.Equal("session-usage", @event.Type);
            Assert.Equal(seconds, @event.Usage!.Seconds);
        }
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData(null, null)]
    [InlineData("\"rejected\"", "rejected")]
    [UpstreamTest(Prefix + "handles an error code of %s and keeps the raw value", Coverage = UpstreamCoverage.Covered)]
    public void Handles_an_error_code_and_keeps_the_raw_value(string? codeJson, string? expectedCode)
    {
        var rawJson = codeJson == null
            ? "{\"type\":\"error\",\"error\":{\"message\":\"Rejected\"}}"
            : "{\"type\":\"error\",\"error\":{\"message\":\"Rejected\",\"code\":" + codeJson + "}}";

        var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson));

        var @event = Assert.Single(parsed);
        Assert.Equal("error", @event.Type);
        Assert.Equal("Rejected", @event.Message);
        Assert.Equal(expectedCode, @event.Code);
        Assert.Null(@event.ClientEventId);
        AssertRaw(rawJson, @event);
    }

    [Theory]
    [InlineData("\"opaque-delegation\"")]
    [InlineData("null")]
    [InlineData(null)]
    [UpstreamTest(Prefix + "preserves response.event as custom with opaque ID %s", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_response_event_as_custom_with_opaque_ID(string? delegationIdJson)
    {
        const string EventJson = "{\"type\":\"response.completed\",\"response\":{\"id\":\"response-1\",\"output\":[],\"usage\":{\"input_tokens\":10}},\"additional\":{\"future\":true}}";
        var rawJson = "{\"type\":\"response.event\","
            + (delegationIdJson == null ? string.Empty : "\"delegation_id\":" + delegationIdJson + ",")
            + "\"event\":" + EventJson + "}";

        var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson));

        var @event = Assert.Single(parsed);
        Assert.Equal("custom", @event.Type);
        Assert.Equal("response.event", @event.RawType);
        AssertRaw(rawJson, @event);
    }

    [Theory]
    [InlineData("future.event")]
    [InlineData("response.completed")]
    [InlineData("response.output_text.delta")]
    [UpstreamTest(Prefix + "preserves unknown %s events as custom", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_unknown_events_as_custom(string type)
    {
        var rawJson = "{\"type\":\"" + type + "\",\"additional\":true}";

        var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson));

        var @event = Assert.Single(parsed);
        Assert.Equal("custom", @event.Type);
        Assert.Equal(type, @event.RawType);
        AssertRaw(rawJson, @event);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"not an event object\"")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"session.started\"}")]
    [InlineData("{\"type\":\"session.started\",\"session\":{\"id\":\"\"}}")]
    [InlineData("{\"type\":\"session.closed\",\"session\":{\"id\":\"session-1\"},\"reason\":\"close_requested\"}")]
    [InlineData("{\"type\":\"session.closed\",\"usage\":{\"seconds\":1}}")]
    [InlineData("{\"type\":\"session.closed\",\"session\":{\"id\":123},\"usage\":{\"seconds\":1},\"reason\":\"expired\"}")]
    [InlineData("{\"type\":\"session.usage.updated\",\"usage\":{\"seconds\":-1}}")]
    [InlineData("{\"type\":\"session.usage.updated\",\"usage\":{\"seconds\":\"1\"}}")]
    [InlineData("{\"type\":\"session.usage.updated\",\"usage\":{\"seconds\":1},\"context_window\":{\"usage_ratio\":\"0.2\"}}")]
    [InlineData("{\"type\":\"session.output_audio.delta\",\"delta\":123}")]
    [InlineData("{\"type\":\"session.input_transcript.delta\",\"delta\":\"hi\"}")]
    [InlineData("{\"type\":\"session.output_transcript.delta\",\"delta\":\"hi\",\"start_ms\":20,\"end_ms\":10}")]
    [InlineData("{\"type\":\"session.delegation.created\",\"delegation\":{}}")]
    [InlineData("{\"type\":\"session.updated\"}")]
    [InlineData("{\"type\":\"session.input_audio.muted\",\"client_event_id\":123}")]
    [InlineData("{\"type\":\"session.instructions.appended\",\"client_event_id\":\"append-1\"}")]
    [InlineData("{\"type\":\"error\",\"error\":{\"message\":\"bad\",\"code\":42}}")]
    [UpstreamTest(Prefix + "reports invalid known events without signaling readiness or finalization: %j", Coverage = UpstreamCoverage.Covered)]
    public void Reports_invalid_known_events_without_signaling_readiness_or_finalization(string rawJson)
    {
        var parsed = s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson));

        var @event = Assert.Single(parsed);
        Assert.Equal("error", @event.Type);
        Assert.Equal("invalid_server_event", @event.Code);
        AssertRaw(rawJson, @event);
    }

    [Theory]
    [InlineData(null, "client")]
    [InlineData("null", "client")]
    [InlineData("{\"type\":\"client\"}", "client")]
    [InlineData("{\"type\":\"responses\"}", "provider")]
    [UpstreamTest(Prefix + "preserves confirmed delegation mode %j as %s", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_confirmed_delegation_mode(string? delegationJson, string delegationMode)
    {
        var rawJson = "{\"type\":\"session.started\",\"session\":{\"id\":\"session-1\""
            + (delegationJson == null ? string.Empty : ",\"delegation\":" + delegationJson)
            + "}}";
        var raw = OpenAIUpstream.Json(rawJson);

        foreach (var parse in new Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>>[] { s_model.CreateServerEventParser(), s_model.ParseServerEvent })
        {
            var @event = Assert.Single(parse(raw));
            Assert.Equal("session-started", @event.Type);
            Assert.Equal("session-1", @event.SessionId);
            Assert.Equal(delegationMode, @event.DelegationMode);
            AssertRaw(rawJson, @event);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "preserves delegation metadata without inventing response identity", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_delegation_metadata_without_inventing_response_identity()
    {
        foreach (var responseId in new string?[] { null, "opaque-response" })
        {
            var rawJson = "{\"type\":\"session.delegation.created\",\"delegation\":{\"id\":\"opaque-delegation\",\"target\":\"responses\""
                + (responseId == null ? string.Empty : ",\"response_id\":\"" + responseId + "\"")
                + "}}";

            var @event = Assert.Single(s_model.ParseServerEvent(OpenAIUpstream.Json(rawJson)));

            Assert.Equal("delegation-created", @event.Type);
            Assert.Equal("opaque-delegation", @event.DelegationId);
            Assert.Equal("provider", @event.Target);
            Assert.Null(@event.OffsetMs);
            Assert.Equal(responseId, @event.ResponseId);
            AssertRaw(rawJson, @event);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "returns fresh pure parsers and preserves unknown envelopes without normalization", Coverage = UpstreamCoverage.Covered)]
    public void Returns_fresh_pure_parsers_and_preserves_unknown_envelopes_without_normalization()
    {
        var first = s_model.CreateServerEventParser();
        var second = s_model.CreateServerEventParser();
        Assert.NotSame(first, second);

        foreach (var rawJson in new[]
        {
            "{\"type\":\"response.event\",\"event\":{\"type\":\"response.created\",\"response\":{\"id\":\"response-1\"}}}",
            "{\"type\":\"response.event\",\"event\":null}",
            "{\"type\":\"response.event\"}",
        })
        {
            var raw = OpenAIUpstream.Json(rawJson);
            foreach (var parse in new Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>>[] { first, second, s_model.ParseServerEvent })
            {
                var @event = Assert.Single(parse(raw));
                Assert.Equal("custom", @event.Type);
                Assert.Equal("response.event", @event.RawType);
                AssertRaw(rawJson, @event);
            }
        }
    }
}
