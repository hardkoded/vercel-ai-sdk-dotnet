// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Mapper = global::Vercel.AI.Xai.XaiRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.Xai.XaiRealtimeEventMapper;

/// <summary>Port of <c>xai-realtime-event-mapper.test.ts</c> &gt; <c>parseXaiRealtimeServerEvent usage</c>.</summary>
public sealed class ParseXaiRealtimeServerEventUsageTests
{
    private const string Id = "packages/xai/src/realtime/xai-realtime-event-mapper.test.ts::parseXaiRealtimeServerEvent usage::";
    private const string Commit = "892af4dab7f1351fd2ced70dcaba288746d8a782";

    [Fact]
    [UpstreamTest(Id + "reads top-level response.done usage even when nested usage is empty", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Reads_top_level_response_done_usage_even_when_nested_usage_is_empty()
    {
        var ev = Parse(
            """
            {"type":"response.done","response":{"id":"response","status":"completed","usage":{}},
            "usage":{"input_tokens":4,"output_tokens":37,"input_token_details":{"text_tokens":4,"audio_tokens":0},
            "output_token_details":{"text_tokens":1,"audio_tokens":36}}}
            """);

        Assert.Equal("response-done", ev.Type);
        AssertUsage(ev.Usage, inputTokens: 4, outputTokens: 37, inputTextTokens: 4, inputAudioTokens: 0, outputTextTokens: 1, outputAudioTokens: 36);
    }

    [Fact]
    [UpstreamTest(Id + "retains duration usage on transcription completions", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Retains_duration_usage_on_transcription_completions()
    {
        var ev = Parse(
            """
            {"type":"conversation.item.input_audio_transcription.completed","item_id":"item","transcript":"hello",
            "usage":{"type":"duration","seconds":2}}
            """);

        Assert.Equal("input-transcription-completed", ev.Type);
        AssertUsage(ev.Usage, seconds: 2);
    }

    [Fact]
    [UpstreamTest(Id + "retains gross totals without inventing modality splits", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Retains_gross_totals_without_inventing_modality_splits()
    {
        var ev = Parse(
            """
            {"type":"response.done","response":{"id":"response","usage":{"input_tokens":10,"output_tokens":0,
            "input_token_details":{"cached_tokens":4,"image_tokens":10}}}}
            """);

        Assert.Equal("response-done", ev.Type);
        AssertUsage(ev.Usage, inputTokens: 10, outputTokens: 0, cachedInputTokens: 4);
    }

    [Fact]
    [UpstreamTest(Id + "extracts gross input/output and separate cached tokens from response.done", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Extracts_gross_input_output_and_separate_cached_tokens_from_response_done()
    {
        var ev = Parse(
            """
            {"type":"response.done","response":{"id":"resp_1","status":"completed","usage":{
            "input_token_details":{"audio_tokens":80,"text_tokens":20,"cached_tokens_details":{"audio_tokens":16,"text_tokens":4}},
            "output_token_details":{"audio_tokens":120,"text_tokens":6}}}}
            """);

        Assert.Equal("response-done", ev.Type);
        Assert.Equal("resp_1", ev.ResponseId);
        AssertUsage(ev.Usage, cachedInputAudioTokens: 16, cachedInputTextTokens: 4, inputAudioTokens: 80, inputTextTokens: 20, outputAudioTokens: 120, outputTextTokens: 6);
    }

    [Fact]
    [UpstreamTest(Id + "omits usage when response.done carries none", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Omits_usage_when_response_done_carries_none()
    {
        var ev = Parse("""{"type":"response.done","response":{"id":"resp_1","status":"completed"}}""");

        Assert.Equal("response-done", ev.Type);
        Assert.Null(ev.Usage);
    }

    private static RealtimeModelV4ServerEvent Parse(string json)
    {
        return Mapper.ParseXaiRealtimeServerEvent(System.Text.Json.JsonDocument.Parse(json).RootElement.Clone());
    }

    private static void AssertUsage(
        RealtimeModelV4Usage? actual,
        int? inputTokens = null,
        int? outputTokens = null,
        int? cachedInputTokens = null,
        int? inputTextTokens = null,
        int? inputAudioTokens = null,
        int? outputTextTokens = null,
        int? outputAudioTokens = null,
        int? cachedInputTextTokens = null,
        int? cachedInputAudioTokens = null,
        double? seconds = null)
    {
        Assert.NotNull(actual);
        Assert.Equal(inputTokens, actual!.InputTokens);
        Assert.Equal(outputTokens, actual.OutputTokens);
        Assert.Equal(cachedInputTokens, actual.CachedInputTokens);
        Assert.Equal(inputTextTokens, actual.InputTextTokens);
        Assert.Equal(inputAudioTokens, actual.InputAudioTokens);
        Assert.Equal(outputTextTokens, actual.OutputTextTokens);
        Assert.Equal(outputAudioTokens, actual.OutputAudioTokens);
        Assert.Equal(cachedInputTextTokens, actual.CachedInputTextTokens);
        Assert.Equal(cachedInputAudioTokens, actual.CachedInputAudioTokens);
        Assert.Equal(seconds, actual.Seconds);
    }
}
