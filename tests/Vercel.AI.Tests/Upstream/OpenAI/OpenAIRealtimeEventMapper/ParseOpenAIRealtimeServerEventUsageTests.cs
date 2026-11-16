// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Mapper = Vercel.AI.OpenAI.OpenAIRealtimeEventMapper;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeEventMapper;

/// <summary>Port of <c>openai-realtime-event-mapper.test.ts</c> &gt; <c>parseOpenAIRealtimeServerEvent usage</c>.</summary>
public sealed class ParseOpenAIRealtimeServerEventUsageTests
{
    private const string Prefix = RealtimeMapperSupport.File + "parseOpenAIRealtimeServerEvent usage::";

    [Fact]
    [UpstreamTest(Prefix + "retains gross totals without inventing modality splits", Coverage = UpstreamCoverage.Covered, Commit = RealtimeMapperSupport.Commit)]
    public void Retains_gross_totals_without_inventing_modality_splits()
    {
        var evt = Mapper.ParseOpenAIRealtimeServerEvent(RealtimeMapperSupport.Json(
            "{\"type\":\"response.done\",\"response\":{\"id\":\"response\",\"usage\":{\"input_tokens\":10,\"output_tokens\":0,\"input_token_details\":{\"cached_tokens\":4,\"image_tokens\":10}}}}"));

        Assert.Equal("response-done", evt.Type);
        OpenAIUpstream.Equal(RealtimeMapperSupport.UsageFields(evt.Usage), "{\"inputTokens\":10,\"outputTokens\":0,\"cachedInputTokens\":4}");
    }

    [Fact]
    [UpstreamTest(Prefix + "extracts gross input/output and separate cached tokens from response.done", Coverage = UpstreamCoverage.Covered, Commit = RealtimeMapperSupport.Commit)]
    public void Extracts_gross_input_output_and_separate_cached_tokens_from_response_done()
    {
        var evt = Mapper.ParseOpenAIRealtimeServerEvent(RealtimeMapperSupport.Json(
            "{\"type\":\"response.done\",\"response\":{\"id\":\"resp_1\",\"status\":\"completed\",\"usage\":{\"input_token_details\":{\"audio_tokens\":100,\"text_tokens\":40,\"cached_tokens_details\":{\"audio_tokens\":30,\"text_tokens\":10}},\"output_token_details\":{\"audio_tokens\":200,\"text_tokens\":12}}}}"));

        Assert.Equal("response-done", evt.Type);
        Assert.Equal("resp_1", evt.ResponseId);
        // Input buckets are gross (cache-inclusive); cached surfaced separately.
        OpenAIUpstream.Equal(
            RealtimeMapperSupport.UsageFields(evt.Usage),
            "{\"cachedInputAudioTokens\":30,\"cachedInputTextTokens\":10,\"inputAudioTokens\":100,\"inputTextTokens\":40,\"outputAudioTokens\":200,\"outputTextTokens\":12}");
    }

    [Fact]
    [UpstreamTest(Prefix + "omits usage when response.done carries none", Coverage = UpstreamCoverage.Covered, Commit = RealtimeMapperSupport.Commit)]
    public void Omits_usage_when_response_done_carries_none()
    {
        var evt = Mapper.ParseOpenAIRealtimeServerEvent(RealtimeMapperSupport.Json(
            "{\"type\":\"response.done\",\"response\":{\"id\":\"resp_1\",\"status\":\"completed\"}}"));

        Assert.Equal("response-done", evt.Type);
        Assert.Null(evt.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "extracts duration usage from a transcription completion", Coverage = UpstreamCoverage.Covered, Commit = RealtimeMapperSupport.Commit)]
    public void Extracts_duration_usage_from_a_transcription_completion()
    {
        var evt = Mapper.ParseOpenAIRealtimeServerEvent(RealtimeMapperSupport.Json(
            "{\"type\":\"conversation.item.input_audio_transcription.completed\",\"item_id\":\"item_1\",\"transcript\":\"hello\",\"usage\":{\"type\":\"duration\",\"seconds\":3.5}}"));

        Assert.Equal("input-transcription-completed", evt.Type);
        OpenAIUpstream.Equal(RealtimeMapperSupport.UsageFields(evt.Usage), "{\"seconds\":3.5}");
    }

    [Fact]
    [UpstreamTest(Prefix + "extracts token usage from a transcription completion", Coverage = UpstreamCoverage.Covered, Commit = RealtimeMapperSupport.Commit)]
    public void Extracts_token_usage_from_a_transcription_completion()
    {
        var evt = Mapper.ParseOpenAIRealtimeServerEvent(RealtimeMapperSupport.Json(
            "{\"type\":\"conversation.item.input_audio_transcription.completed\",\"item_id\":\"item_1\",\"transcript\":\"hello\",\"usage\":{\"type\":\"tokens\",\"input_token_details\":{\"audio_tokens\":50,\"text_tokens\":4},\"output_tokens\":8}}"));

        Assert.Equal("input-transcription-completed", evt.Type);
        OpenAIUpstream.Equal(
            RealtimeMapperSupport.UsageFields(evt.Usage),
            "{\"inputAudioTokens\":50,\"inputTextTokens\":4,\"outputTextTokens\":8,\"outputTokens\":8}");
    }
}
