// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning summary text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning_summary_text_deltas()
    {
        var capture = new UpstreamCapture();
        PrepareChunksFixtureResponse(capture, "openai-reasoning-summary-text.1");

        var parts = await UpstreamChat.Read(CreateModel(capture).DoStreamAsync(TestPrompt(), CancellationToken.None));

        var reasoning = parts.Where(part => part.Type.StartsWith("reasoning", StringComparison.Ordinal)).ToList();
        Assert.Equal(4, reasoning.Count);
        Assert.Equal("rs_1", Assert.IsType<ReasoningStartStreamPart>(reasoning[0]).Id);
        var first = Assert.IsType<ReasoningDeltaStreamPart>(reasoning[1]);
        Assert.Equal(("rs_1", "Think"), (first.Id, first.Delta));
        var second = Assert.IsType<ReasoningDeltaStreamPart>(reasoning[2]);
        Assert.Equal(("rs_1", "ing."), (second.Id, second.Delta));
        var end = Assert.IsType<ReasoningEndStreamPart>(reasoning[3]);
        Assert.Equal("rs_1", end.Id);
        AssertMetadata(
            "{\"lmstudio\":{\"itemId\":\"rs_1\",\"reasoningSummary\":[{\"type\":\"summary_text\",\"text\":\"Thinking.\"}],\"reasoningContent\":null}}",
            end.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "should decode registered extension events and completed items", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_decode_registered_extension_events_and_completed_items()
    {
        const string receiptJson = "{\"id\":\"search_stream_1\",\"type\":\"acme:document_search_receipt\",\"status\":\"completed\",\"call_id\":\"call_stream_1\",\"name\":\"documentSearch\",\"query\":{\"text\":\"streamed query\"},\"result\":{\"documents\":[\"doc_1\"]},\"opaque_receipt\":{\"cursor\":\"cursor_1\"}}";
        const string reference = "{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"itemId\":\"search_stream_1\"}}}";
        var capture = new UpstreamCapture();
        PrepareStreamChunks(
            capture,
            Obj("{\"type\":\"acme:document_search_input\",\"sequence_number\":0,\"call_id\":\"call_stream_1\",\"name\":\"documentSearch\",\"delta\":\"{\\\"text\\\":\\\"streamed query\\\"}\"}"),
            Obj("{\"type\":\"response.output_item.done\",\"sequence_number\":1,\"output_index\":0,\"item\":" + receiptJson + "}"),
            Obj("{\"type\":\"response.completed\",\"sequence_number\":2,\"response\":{\"id\":\"response_stream_1\",\"object\":\"response\",\"created_at\":0,\"status\":\"completed\",\"model\":\"test-model\",\"output\":[" + receiptJson + "],\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}}"));

        var parts = await UpstreamChat.Read(
            CreateModel(capture, "gemma-7b-it", CreateDocumentSearchExtension(providerExecuted: true)).DoStreamAsync(TestPrompt(), CancellationToken.None));

        var start = Assert.Single(parts.OfType<ToolInputStartStreamPart>());
        Assert.Equal(("call_stream_1", "documentSearch", true), (start.Id, start.ToolName, start.ProviderExecuted));
        var delta = Assert.Single(parts.OfType<ToolInputDeltaStreamPart>());
        Assert.Equal(("call_stream_1", "{\"text\":\"streamed query\"}"), (delta.Id, delta.Delta));
        var replay = Assert.Single(parts.OfType<CustomStreamPart>());
        Assert.Equal("open-responses.extension-replay", replay.Kind);
        AssertMetadata("{\"lmstudio\":{\"openResponsesExtension\":{\"id\":\"acme.document_search\",\"item\":" + receiptJson + "}}}", replay.ProviderMetadata);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(("call_stream_1", "documentSearch", "{\"text\":\"streamed query\"}", true), (call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderExecuted));
        AssertMetadata(reference, call.ProviderMetadata);
        var result = Assert.Single(parts.OfType<ToolResultStreamPart>());
        Assert.Equal(("call_stream_1", "documentSearch"), (result.ToolCallId, result.ToolName));
        AssertJson(Obj("{\"documents\":[\"doc_1\"]}"), JsonNode.Parse(result.Result.GetRawText()));
        AssertMetadata(reference, result.ProviderMetadata);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve output text annotations in stream metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_output_text_annotations_in_stream_metadata()
    {
        const string annotation = "{\"type\":\"url_citation\",\"start_index\":0,\"end_index\":7,\"url\":\"https://example.com/source\",\"title\":\"Example source\"}";
        var capture = new UpstreamCapture();
        PrepareStreamChunks(
            capture,
            Obj("{\"type\":\"response.output_item.added\",\"sequence_number\":0,\"output_index\":0,\"item\":{\"id\":\"msg_annotated\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"in_progress\",\"content\":[]}}"),
            Obj("{\"type\":\"response.output_text.delta\",\"sequence_number\":1,\"item_id\":\"msg_annotated\",\"output_index\":0,\"content_index\":0,\"delta\":\"Sourced answer\"}"),
            Obj("{\"type\":\"response.output_item.done\",\"sequence_number\":2,\"output_index\":0,\"item\":{\"id\":\"msg_annotated\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"Sourced answer\",\"annotations\":[" + annotation + "]}]}}"));

        var parts = await UpstreamChat.Read(CreateModel(capture).DoStreamAsync(TestPrompt(), CancellationToken.None));

        var end = Assert.Single(parts.OfType<TextEndStreamPart>());
        Assert.Equal("msg_annotated", end.Id);
        AssertMetadata("{\"lmstudio\":{\"itemId\":\"msg_annotated\",\"annotations\":[" + annotation + "]}}", end.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "should close unfinished reasoning items with their original ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_close_unfinished_reasoning_items_with_their_original_ids()
    {
        var capture = new UpstreamCapture();
        PrepareStreamChunks(
            capture,
            Obj("{\"type\":\"response.output_item.added\",\"sequence_number\":0,\"output_index\":0,\"item\":{\"id\":\"rs_reasoning_item\",\"type\":\"reasoning\",\"summary\":[]}}"),
            Obj("{\"type\":\"response.incomplete\",\"sequence_number\":1,\"response\":{\"status\":\"incomplete\",\"incomplete_details\":{\"reason\":\"max_output_tokens\"},\"usage\":{\"input_tokens\":1,\"input_tokens_details\":{\"cached_tokens\":0},\"output_tokens\":1,\"output_tokens_details\":{\"reasoning_tokens\":1},\"total_tokens\":2}}}"));

        var parts = await UpstreamChat.Read(CreateModel(capture).DoStreamAsync(TestPrompt(), CancellationToken.None));

        Assert.Equal("rs_reasoning_item", Assert.Single(parts.OfType<ReasoningStartStreamPart>()).Id);
        var end = Assert.Single(parts.OfType<ReasoningEndStreamPart>());
        Assert.Equal("rs_reasoning_item", end.Id);
        Assert.Null(end.ProviderMetadata);
        var finish = Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
        Assert.Equal((FinishReason.Length, "max_output_tokens"), (finish.FinishReason, finish.RawFinishReason));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send provider-native reasoning effort when streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_provider_native_reasoning_effort_when_streaming()
    {
        var capture = new UpstreamCapture();
        PrepareChunksFixtureResponse(capture, "lmstudio-basic.1");
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("lmstudio", "{\"reasoningEffort\":\"max\"}");

        await UpstreamChat.Read(CreateModel(capture).DoStreamAsync(options, CancellationToken.None));

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }
}
