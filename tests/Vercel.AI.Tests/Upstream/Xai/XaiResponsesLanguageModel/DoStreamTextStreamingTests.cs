// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel.XaiResponsesLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesLanguageModel;

/// <summary>Port of <c>xai-responses-language-model.test.ts</c> &gt; <c>XaiResponsesLanguageModel &gt; doStream &gt; text streaming</c>.</summary>
public sealed class DoStreamTextStreamingTests
{
    private const string Prefix = TestFile + " > doStream > text streaming::";
    private const string SnapshotNote = "Asserts part order, ids, counts, and the finish part, not each delta. reasoning-delta parts carry no provider metadata.";

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas", Coverage = UpstreamCoverage.Partial, Note = SnapshotNote)]
    public async Task Should_stream_text_deltas()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-streaming.1.chunks.txt")), HelloPrompt());

        AssertStream(
            parts,
            id: "769f3302-64f9-4c72-2b48-860c87fd9b2a",
            timestamp: new DateTimeOffset(2025, 11, 22, 23, 18, 20, TimeSpan.Zero),
            reasoningDeltas: 59,
            textDeltas: 626,
            reasoningTokens: 237,
            outputTokens: 863);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas with reasoning", Coverage = UpstreamCoverage.Partial, Note = SnapshotNote)]
    public async Task Should_stream_text_deltas_with_reasoning()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-with-reasoning-streaming.1.chunks.txt")), HelloPrompt());

        AssertStream(
            parts,
            id: "bf3b2b34-79d4-a45c-7be8-d1e5f96386c2",
            timestamp: new DateTimeOffset(2025, 11, 22, 23, 54, 28, TimeSpan.Zero),
            reasoningDeltas: 66,
            textDeltas: 600,
            reasoningTokens: 323,
            outputTokens: 923);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas with encrypted reasoning", Coverage = UpstreamCoverage.Partial, Note = SnapshotNote)]
    public async Task Should_stream_text_deltas_with_encrypted_reasoning()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-with-reasoning-streaming-store-false.1.chunks.txt")), HelloPrompt());

        AssertStream(
            parts,
            id: "0b824fe9-3250-2588-0bbf-0810402fc822",
            timestamp: new DateTimeOffset(2025, 11, 23, 1, 11, 25, TimeSpan.Zero),
            reasoningDeltas: 64,
            textDeltas: 578,
            reasoningTokens: 253,
            outputTokens: 831);
        var end = First<ReasoningEndStreamPart>(parts);
        var xai = end.ProviderMetadata!.Value.GetProperty("xai");
        Assert.Equal("rs_0b824fe9-3250-2588-0bbf-0810402fc822", xai.GetProperty("itemId").GetString());
        var encrypted = xai.GetProperty("reasoningEncryptedContent").GetString()!;
        Assert.Equal(1731, encrypted.Length);
        Assert.StartsWith("WjtnRFdCMihg", encrypted, StringComparison.Ordinal);
        Assert.EndsWith("jndXaAUM", encrypted, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include encrypted content in reasoning-end providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_encrypted_content_in_reasoning_end_providerMetadata()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.output_item.added","item":{"type":"reasoning","id":"rs_456","status":"in_progress","summary":[]},"output_index":0}""",
            """{"type":"response.reasoning_summary_part.added","item_id":"rs_456","output_index":0,"summary_index":0,"part":{"type":"summary_text","text":""}}""",
            """{"type":"response.reasoning_summary_text.delta","item_id":"rs_456","output_index":0,"summary_index":0,"delta":"Analyzing..."}""",
            """{"type":"response.reasoning_summary_text.done","item_id":"rs_456","output_index":0,"summary_index":0,"text":"Analyzing..."}""",
            """{"type":"response.output_item.done","item":{"type":"reasoning","id":"rs_456","status":"completed","summary":[{"type":"summary_text","text":"Analyzing..."}],"encrypted_content":"encrypted_data_abc123"},"output_index":0}""",
            """{"type":"response.output_item.added","item":{"type":"message","id":"msg_789","role":"assistant","status":"in_progress","content":[]},"output_index":1}""",
            """{"type":"response.output_text.delta","item_id":"msg_789","output_index":1,"content_index":0,"delta":"Result."}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":20}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var end = First<ReasoningEndStreamPart>(parts);
        Assert.Equal("reasoning-rs_456", end.Id);
        JsonAssert.Equal(end.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456","reasoningEncryptedContent":"encrypted_data_abc123"}}""");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit reasoning-start before reasoning-end when reasoning_summary_part.added is not sent",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_reasoning_start_before_reasoning_end_when_reasoning_summary_part_added_is_not_sent()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","output":[]}}""",
            """{"type":"response.output_item.added","item":{"type":"reasoning","id":"rs_456","status":"in_progress","summary":[]},"output_index":0}""",
            """{"type":"response.output_item.done","item":{"type":"reasoning","id":"rs_456","status":"completed","summary":[],"encrypted_content":"encrypted_reasoning_content_xyz"},"output_index":0}""",
            """{"type":"response.output_item.added","item":{"type":"message","id":"msg_789","role":"assistant","status":"in_progress","content":[]},"output_index":1}""",
            """{"type":"response.output_text.delta","item_id":"msg_789","output_index":1,"content_index":0,"delta":"The answer is 42."}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":20}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var start = First<ReasoningStartStreamPart>(parts);
        Assert.Equal("reasoning-rs_456", start.Id);
        JsonAssert.Equal(start.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
        var end = First<ReasoningEndStreamPart>(parts);
        Assert.Equal("reasoning-rs_456", end.Id);
        JsonAssert.Equal(end.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456","reasoningEncryptedContent":"encrypted_reasoning_content_xyz"}}""");
        Assert.True(parts.IndexOf(start) < parts.IndexOf(end));
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning text deltas (response.reasoning_text.delta)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_text_deltas_response_reasoning_text_delta()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-code-fast-1","output":[]}}""",
            """{"type":"response.output_item.added","item":{"type":"reasoning","id":"rs_456","status":"in_progress","summary":[]},"output_index":0}""",
            """{"type":"response.reasoning_text.delta","item_id":"rs_456","output_index":0,"content_index":0,"delta":"First"}""",
            """{"type":"response.reasoning_text.delta","item_id":"rs_456","output_index":0,"content_index":0,"delta":", analyze the question."}""",
            """{"type":"response.reasoning_text.done","item_id":"rs_456","output_index":0,"content_index":0,"text":"First, analyze the question."}""",
            """{"type":"response.output_item.done","item":{"type":"reasoning","id":"rs_456","status":"completed","summary":[{"type":"summary_text","text":"First, analyze the question."}]},"output_index":0}""",
            """{"type":"response.output_item.added","item":{"type":"message","id":"msg_789","role":"assistant","status":"in_progress","content":[]},"output_index":1}""",
            """{"type":"response.output_text.delta","item_id":"msg_789","output_index":1,"content_index":0,"delta":"The answer."}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","model":"grok-code-fast-1","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":20,"output_tokens_details":{"reasoning_tokens":15}}}}""");

        var parts = await Collect(Model(capture, "grok-code-fast-1"), HelloPrompt());

        var start = First<ReasoningStartStreamPart>(parts);
        Assert.Equal("reasoning-rs_456", start.Id);
        JsonAssert.Equal(start.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
        var deltas = parts.OfType<ReasoningDeltaStreamPart>().ToList();
        Assert.Equal(new[] { "First", ", analyze the question." }, deltas.Select(delta => delta.Delta));
        var end = First<ReasoningEndStreamPart>(parts);
        Assert.Equal("reasoning-rs_456", end.Id);
        JsonAssert.Equal(end.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
        var startIndex = parts.IndexOf(start);
        var firstDeltaIndex = parts.IndexOf(deltas[0]);
        var endIndex = parts.IndexOf(end);
        var textIndex = parts.IndexOf(First<TextDeltaStreamPart>(parts));
        Assert.True(startIndex < firstDeltaIndex);
        Assert.True(firstDeltaIndex < endIndex);
        Assert.True(endIndex < textIndex);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should emit only one reasoning-start when multiple reasoning_summary_part.added events share an item_id",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_emit_only_one_reasoning_start_when_multiple_reasoning_summary_part_added_events_share_an_item_id()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4.3","output":[]}}""",
            """{"type":"response.output_item.added","item":{"type":"reasoning","id":"rs_456","status":"in_progress","summary":[]},"output_index":0}""",
            """{"type":"response.reasoning_summary_part.added","item_id":"rs_456","output_index":0,"summary_index":0,"part":{"type":"summary_text","text":""}}""",
            """{"type":"response.reasoning_summary_text.delta","item_id":"rs_456","output_index":0,"summary_index":0,"delta":"First summary part."}""",
            """{"type":"response.reasoning_summary_part.added","item_id":"rs_456","output_index":0,"summary_index":1,"part":{"type":"summary_text","text":""}}""",
            """{"type":"response.reasoning_summary_text.delta","item_id":"rs_456","output_index":0,"summary_index":1,"delta":" Second summary part."}""",
            """{"type":"response.output_item.done","item":{"type":"reasoning","id":"rs_456","status":"completed","summary":[{"type":"summary_text","text":"First summary part."},{"type":"summary_text","text":" Second summary part."}]},"output_index":0}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","model":"grok-4.3","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":20}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var starts = parts.OfType<ReasoningStartStreamPart>().ToList();
        Assert.Single(parts.OfType<ReasoningEndStreamPart>());
        var start = Assert.Single(starts);
        Assert.Equal("reasoning-rs_456", start.Id);
        JsonAssert.Equal(start.ProviderMetadata!.Value, """{"xai":{"itemId":"rs_456"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not emit duplicate text-delta from response.output_item.done after streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_emit_duplicate_text_delta_from_response_output_item_done_after_streaming()
    {
        var capture = StreamResponse(
            """{"type":"response.created","response":{"id":"resp_123","object":"response","model":"grok-4-fast-non-reasoning","created_at":1700000000,"status":"in_progress","output":[]}}""",
            """{"type":"response.output_item.added","item":{"type":"message","id":"msg_123","status":"in_progress","role":"assistant","content":[]},"output_index":0}""",
            """{"type":"response.output_text.delta","item_id":"msg_123","output_index":0,"content_index":0,"delta":"Hello"}""",
            """{"type":"response.output_text.delta","item_id":"msg_123","output_index":0,"content_index":0,"delta":" "}""",
            """{"type":"response.output_text.delta","item_id":"msg_123","output_index":0,"content_index":0,"delta":"world"}""",
            """{"type":"response.output_item.done","item":{"type":"message","id":"msg_123","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Hello world","annotations":[]}]},"output_index":0}""",
            """{"type":"response.done","response":{"id":"resp_123","object":"response","status":"completed","output":[],"usage":{"input_tokens":10,"output_tokens":5}}}""");

        var parts = await Collect(Model(capture), HelloPrompt());

        var deltas = parts.OfType<TextDeltaStreamPart>().ToList();
        Assert.Equal(new[] { "Hello", " ", "world" }, deltas.Select(delta => delta.Delta));
        Assert.DoesNotContain(deltas, delta => delta.Delta == "Hello world");
    }

    private static void AssertStream(
        List<LanguageModelStreamPart> parts,
        string id,
        DateTimeOffset timestamp,
        int reasoningDeltas,
        int textDeltas,
        int reasoningTokens,
        int outputTokens)
    {
        // The recorded stream: start, metadata, one reasoning block, one text block, finish.
        var types = new List<string>();
        foreach (var part in parts)
        {
            if (types.Count == 0 || types[^1] != part.Type)
            {
                types.Add(part.Type);
            }
        }

        Assert.Equal(
            new[] { "stream-start", "response-metadata", "reasoning-start", "reasoning-delta", "reasoning-end", "text-start", "text-delta", "text-end", "finish" },
            types);
        Assert.Empty(First<StreamStartStreamPart>(parts).Warnings);
        var metadata = First<ResponseMetadataStreamPart>(parts);
        Assert.Equal(id, metadata.Id);
        Assert.Equal("grok-code-fast-1", metadata.ModelId);
        Assert.Equal(timestamp, metadata.Timestamp);
        var reasoningStart = First<ReasoningStartStreamPart>(parts);
        Assert.Equal("reasoning-rs_" + id, reasoningStart.Id);
        JsonAssert.Equal(reasoningStart.ProviderMetadata!.Value, "{\"xai\":{\"itemId\":\"rs_" + id + "\"}}");
        Assert.Equal(reasoningDeltas, parts.OfType<ReasoningDeltaStreamPart>().Count());
        Assert.Equal("reasoning-rs_" + id, First<ReasoningEndStreamPart>(parts).Id);
        Assert.Equal("text-msg_" + id, First<TextStartStreamPart>(parts).Id);
        Assert.Equal(textDeltas, parts.OfType<TextDeltaStreamPart>().Count());
        Assert.Equal("text-msg_" + id, First<TextEndStreamPart>(parts).Id);
        var finish = First<FinishStreamPart>(parts);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("completed", finish.RawFinishReason);
        Assert.Equal(216, finish.Usage.InputTokens);
        Assert.Equal(24, finish.Usage.NoCacheInputTokens);
        Assert.Equal(192, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(outputTokens, finish.Usage.OutputTokens);
        Assert.Equal(reasoningTokens, finish.Usage.ReasoningTokens);
        Assert.Equal(textDeltas, finish.Usage.TextTokens);
    }
}
