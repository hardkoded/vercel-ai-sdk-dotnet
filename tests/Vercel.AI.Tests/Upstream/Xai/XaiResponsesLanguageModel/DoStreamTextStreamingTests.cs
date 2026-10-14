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

    [Fact]
    [UpstreamTest(Prefix + "should stream web search with real response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_web_search_with_real_response()
    {
        var parts = await Collect(
            Model(ChunksFixture("xai-web-search-tool.1.chunks.txt")),
            WithTools(ProviderTool("xai.web_search", "web_search")));

        JsonAssert.Equal(ToJson(parts), Expected("xai-web-search-tool.1.stream.json").ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-streaming.1.chunks.txt")), HelloPrompt());

        JsonAssert.Equal(ToJson(parts), Expected("xai-text-streaming.1.stream.json").ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas with reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas_with_reasoning()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-with-reasoning-streaming.1.chunks.txt")), HelloPrompt());

        JsonAssert.Equal(ToJson(parts), Expected("xai-text-with-reasoning-streaming.1.stream.json").ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream text deltas with encrypted reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_text_deltas_with_encrypted_reasoning()
    {
        var parts = await Collect(Model(ChunksFixture("xai-text-with-reasoning-streaming-store-false.1.chunks.txt")), HelloPrompt());

        JsonAssert.Equal(ToJson(parts), Expected("xai-text-with-reasoning-streaming-store-false.1.stream.json").ToJsonString());
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
    [UpstreamTest(Prefix + "should stream x_search tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_x_search_tool_call()
    {
        var parts = await Collect(
            Model(ChunksFixture("xai-x-search-tool.chunks.txt")),
            WithTools(ProviderTool("xai.x_search", "x_search")));

        JsonAssert.Equal(ToJson(parts), Expected("xai-x-search-tool.stream.json").ToJsonString());
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
}
