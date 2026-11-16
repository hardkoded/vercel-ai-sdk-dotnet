// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Mapper = global::Vercel.AI.Google.GoogleRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeEventMapper;

/// <summary>Port of <c>google-realtime-event-mapper.test.ts</c> &gt; <c>GoogleRealtimeEventMapper usage</c>.</summary>
public sealed class GoogleRealtimeEventMapperUsageTests
{
    private const string Id = "packages/google/src/realtime/google-realtime-event-mapper.test.ts::GoogleRealtimeEventMapper usage::";

    public static TheoryData<string> NonTerminalFrames
    {
        get
        {
            return new TheoryData<string>
            {
                """{"setupComplete":true}""",
                """{"toolCall":{"functionCalls":[{"id":"call","name":"tool"}]}}""",
                """{"toolCallCancellation":{"ids":["call"]}}""",
                """{"inputTranscription":{"text":"hello"}}""",
                """{"serverContent":{"interrupted":true}}""",
            };
        }
    }

    [Fact]
    [UpstreamTest(Id + "includes reported thinking, tool-use and cache counts without changing modality subsets", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Includes_reported_thinking_tool_use_and_cache_counts_without_changing_modality_subsets()
    {
        var mapper = new Mapper();
        var events = mapper.Parse(
            """
            {"serverContent":{"turnComplete":true},"usageMetadata":{"promptTokenCount":30,"responseTokenCount":9,"toolUsePromptTokenCount":3,
            "thoughtsTokenCount":7,"cachedContentTokenCount":8,"promptTokensDetails":[{"modality":"AUDIO","tokenCount":30}],
            "responseTokensDetails":[{"modality":"AUDIO","tokenCount":9}]}}
            """);

        var done = Assert.Single(events, event_ => event_.Type == "response-done");
        AssertUsage(done.Usage, inputTokens: 33, outputTokens: 16, cachedInputTokens: 8, inputAudioTokens: 30, outputAudioTokens: 9);
    }

    [Fact]
    [UpstreamTest(Id + "does not invent gross totals from partial thinking or tool-use counts", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Does_not_invent_gross_totals_from_partial_thinking_or_tool_use_counts()
    {
        var mapper = new Mapper();
        var event_ = mapper.ParseOne("""{"usageMetadata":{"toolUsePromptTokenCount":3,"thoughtsTokenCount":7,"cachedContentTokenCount":0}}""");

        Assert.NotNull(event_.Usage);
        Assert.Equal(0, event_.Usage!.CachedInputTokens);
        Assert.Null(event_.Usage.InputTokens);
        Assert.Null(event_.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(Id + "preserves totals and known zeros without inventing missing modality counts", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Preserves_totals_and_known_zeros_without_inventing_missing_modality_counts()
    {
        var mapper = new Mapper();
        var event_ = mapper.ParseOne(
            """
            {"usageMetadata":{"promptTokenCount":10,"responseTokenCount":0,
            "promptTokensDetails":[{"modality":"AUDIO"},{"modality":"TEXT","tokenCount":0},{"modality":"IMAGE","tokenCount":10}],
            "responseTokensDetails":[{"modality":"AUDIO","tokenCount":null}]}}
            """);

        Assert.Equal("custom", event_.Type);
        AssertUsage(event_.Usage, inputTokens: 10, outputTokens: 0, inputTextTokens: 0);
    }

    [Fact]
    [UpstreamTest(Id + "attaches per-modality usage to response-done on turnComplete", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Attaches_per_modality_usage_to_response_done_on_turnComplete()
    {
        var mapper = new Mapper();
        var events = mapper.Parse(
            """
            {"serverContent":{"turnComplete":true},"usageMetadata":{"promptTokenCount":60,"responseTokenCount":200,
            "promptTokensDetails":[{"modality":"AUDIO","tokenCount":50},{"modality":"TEXT","tokenCount":10}],
            "responseTokensDetails":[{"modality":"AUDIO","tokenCount":200}]}}
            """);

        var done = Assert.Single(events, event_ => event_.Type == "response-done");
        AssertUsage(done.Usage, inputTokens: 60, outputTokens: 200, inputAudioTokens: 50, inputTextTokens: 10, outputAudioTokens: 200);
    }

    [Fact]
    [UpstreamTest(Id + "preserves aggregate counts without inventing modality details", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Preserves_aggregate_counts_without_inventing_modality_details()
    {
        var mapper = new Mapper();
        var events = mapper.Parse("""{"serverContent":{"turnComplete":true},"usageMetadata":{"promptTokenCount":12,"responseTokenCount":34}}""");

        var done = Assert.Single(events, event_ => event_.Type == "response-done");
        AssertUsage(done.Usage, inputTokens: 12, outputTokens: 34);
    }

    [Theory]
    [MemberData(nameof(NonTerminalFrames))]
    [UpstreamTest(Id + "retains usage alongside non-terminal messages: %j", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Retains_usage_alongside_non_terminal_messages(string frame)
    {
        var mapper = new Mapper();
        var node = JsonNode.Parse(frame)!.AsObject();
        node["usageMetadata"] = JsonNode.Parse("""{"promptTokensDetails":[{"modality":"TEXT","tokenCount":12}]}""");
        var raw = node.ToJsonString();

        var list = mapper.Parse(raw);

        var update = Assert.Single(list, event_ => event_.Type == "custom" && event_.RawType == "usageMetadata");
        Assert.Null(update.ResponseId);
        Assert.Null(update.ItemId);
        Assert.Null(update.Delta);
        AssertUsage(update.Usage, inputTextTokens: 12);
        GoogleUpstream.JsonEqual(update.Raw!.Value, raw);
        Assert.DoesNotContain(list, event_ => event_.Type == "response-done");
    }

    [Fact]
    [UpstreamTest(Id + "keeps unassociated usage snapshots separate from completion and later turns", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Keeps_unassociated_usage_snapshots_separate_from_completion_and_later_turns()
    {
        var mapper = new Mapper();
        static string Snapshot(int count)
        {
            return "{\"usageMetadata\":{\"promptTokensDetails\":[{\"modality\":\"TEXT\",\"tokenCount\":" + count + "}]}}";
        }

        var first = mapper.ParseOne(Snapshot(12));
        Assert.Equal("custom", first.Type);
        AssertUsage(first.Usage, inputTextTokens: 12);

        var done = mapper.Parse("""{"serverContent":{"turnComplete":true}}""");
        Assert.Contains(done, event_ => event_.Type == "response-done");
        Assert.DoesNotContain(done, event_ => event_.Usage != null);

        // A late final snapshot is retained instead of discarded as a duplicate.
        var late = mapper.ParseOne(Snapshot(15));
        Assert.Equal("custom", late.Type);
        Assert.Equal("usageMetadata", late.RawType);
        AssertUsage(late.Usage, inputTextTokens: 15);

        mapper.Parse("""{"toolCall":{"functionCalls":[]}}""");
        var afterToolCall = mapper.ParseOne(Snapshot(16));
        Assert.Equal("custom", afterToolCall.Type);
        AssertUsage(afterToolCall.Usage, inputTextTokens: 16);

        mapper.Parse("""{"serverContent":{"interrupted":true}}""");
        var afterInterrupt = mapper.ParseOne(Snapshot(17));
        Assert.Equal("custom", afterInterrupt.Type);
        AssertUsage(afterInterrupt.Usage, inputTextTokens: 17);

        var next = mapper.Parse("""{"serverContent":{"modelTurn":{"parts":[{"text":"next"}]},"turnComplete":true}}""");
        Assert.DoesNotContain(next, event_ => event_.Usage != null);
    }

    [Fact]
    [UpstreamTest(Id + "omits usage when no usageMetadata is present", Coverage = UpstreamCoverage.Covered, Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782")]
    public void Omits_usage_when_no_usageMetadata_is_present()
    {
        var mapper = new Mapper();
        var events = mapper.Parse("""{"serverContent":{"turnComplete":true}}""");

        var done = Assert.Single(events, event_ => event_.Type == "response-done");
        Assert.Null(done.Usage);
    }

    private static void AssertUsage(
        RealtimeModelV4Usage? actual,
        int? inputTokens = null,
        int? outputTokens = null,
        int? cachedInputTokens = null,
        int? inputTextTokens = null,
        int? inputAudioTokens = null,
        int? outputTextTokens = null,
        int? outputAudioTokens = null)
    {
        Assert.NotNull(actual);
        Assert.Equal(inputTokens, actual!.InputTokens);
        Assert.Equal(outputTokens, actual.OutputTokens);
        Assert.Equal(cachedInputTokens, actual.CachedInputTokens);
        Assert.Equal(inputTextTokens, actual.InputTextTokens);
        Assert.Equal(inputAudioTokens, actual.InputAudioTokens);
        Assert.Equal(outputTextTokens, actual.OutputTextTokens);
        Assert.Equal(outputAudioTokens, actual.OutputAudioTokens);
        Assert.Null(actual.CachedInputTextTokens);
        Assert.Null(actual.CachedInputAudioTokens);
        Assert.Null(actual.Seconds);
    }
}
