// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch.AnthropicBatchTestSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicBatch;

public sealed class AnthropicBatchBatchResultLifecycleTests
{
    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects result retrieval while the batch is pending", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_result_retrieval_while_the_batch_is_pending()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("processing_status", "in_progress"), ("request_counts", Counts(1, 0, 0, 0, 0)), ("results_url", null)));

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => CreateBatch(handler).DoGetResultsAsync(OperationCall(), CancellationToken.None));

        Assert.Equal("batchId", error.Parameter);
        Assert.EndsWith("Anthropic batch \"msgbatch_123\" is not complete.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::fails an invalid item and continues with later results", Coverage = UpstreamCoverage.Covered)]
    public async Task Fails_an_invalid_item_and_continues_with_later_results()
    {
        var results = await GetResultsAsync(
            new RecordingHandler(),
            SucceededLine("invalid", new JsonObject { ["type"] = "message" }) + "\n",
            SucceededLine("valid", MessageResultBody("Paris")));

        Assert.Equal(2, results.Count);
        AssertInvalid(results[0], "invalid");
        Assert.Equal("valid", results[1].Id);
        var text = Assert.Single(Succeeded(results[1]).Content);
        Assert.Equal(("text", "Paris"), (text.Type, text.Text));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::preserves successful messages with nullable and newer citation shapes", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_successful_messages_with_nullable_and_newer_citation_shapes()
    {
        var message = MessageWith(
            "Cited answer",
            JsonNode.Parse("""
                [
                  {"type":"text","text":"No citations","citations":null},
                  {"type":"text","text":"New citations","citations":[
                    {"type":"content_block_location","cited_text":"block","document_index":0,"document_title":null,"start_block_index":0,"end_block_index":1,"file_id":null},
                    {"type":"search_result_location","cited_text":"search result","search_result_index":0,"source":"https://example.com","title":null,"start_block_index":0,"end_block_index":1}
                  ]}
                ]
                """)!.AsArray());

        var results = await GetResultsAsync(new RecordingHandler(), SucceededLine("citation-shapes", message));

        var item = Assert.Single(results);
        Assert.Equal("citation-shapes", item.Id);
        var result = Succeeded(item);
        Assert.Equal([("text", "No citations"), ("text", "New citations")], result.Content.Select(part => (part.Type, part.Text)));
        Assert.Equal(13, result.Usage!.InputTotal);
        Assert.Equal(3, result.Usage.OutputTotal);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::omits empty and null compaction blocks from successful results", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_empty_and_null_compaction_blocks_from_successful_results()
    {
        var message = MessageWith(
            "Done",
            JsonNode.Parse("""[{"type":"compaction","content":""},{"type":"compaction","content":null},{"type":"compaction","content":"Summary"},{"type":"text","text":"Done"}]""")!.AsArray());

        var results = await GetResultsAsync(new RecordingHandler(), SucceededLine("compaction", message));

        var item = Assert.Single(results);
        Assert.Equal("compaction", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(2, content.Count);
        Assert.Equal(("text", "Summary"), (content[0].Type, content[0].Text));
        Assert.Equal("compaction", content[0].ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("type").GetString());
        Assert.Equal(("text", "Done"), (content[1].Type, content[1].Text));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::preserves fallback blocks in successful results", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_fallback_blocks_in_successful_results()
    {
        var message = MessageWith(
            "Done",
            JsonNode.Parse("""[{"type":"fallback","from":{"model":"claude-opus-5-5"},"to":{"model":"claude-opus-4-8"}},{"type":"text","text":"Done"}]""")!.AsArray());

        var results = await GetResultsAsync(new RecordingHandler(), SucceededLine("fallback", message));

        var item = Assert.Single(results);
        Assert.Equal("fallback", item.Id);
        var content = Succeeded(item).Content;
        Assert.Equal(2, content.Count);
        Assert.Equal(("custom", "anthropic.fallback"), (content[0].Type, content[0].Kind));
        AssertJson("""{"anthropic":{"type":"fallback","from":{"model":"claude-opus-5-5"},"to":{"model":"claude-opus-4-8"}}}""", content[0].ProviderMetadata);
        Assert.Equal(("text", "Done"), (content[1].Type, content[1].Text));
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::does not fail a successful message for a container upload block", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_fail_a_successful_message_for_a_container_upload_block()
    {
        var message = MessageWith(
            "Done",
            JsonNode.Parse("""[{"type":"text","text":"Done"},{"type":"container_upload","file_id":"file_123"}]""")!.AsArray());

        var results = await GetResultsAsync(new RecordingHandler(), SucceededLine("container-upload", message));

        var item = Assert.Single(results);
        Assert.Equal("container-upload", item.Id);
        var result = Succeeded(item);
        Assert.Equal(2, result.Content.Count);
        Assert.Equal(("text", "Done"), (result.Content[0].Type, result.Content[0].Text));
        Assert.Equal(("custom", "anthropic.container_upload"), (result.Content[1].Type, result.Content[1].Kind));
        AssertJson("""{"anthropic":{"fileId":"file_123"}}""", result.Content[1].ProviderMetadata);
        Assert.Equal(13, result.Usage!.InputTotal);
        Assert.Equal(3, result.Usage.OutputTotal);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::rejects a completed batch without output", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_completed_batch_without_output()
    {
        var handler = new RecordingHandler().Respond(BatchResponse(("results_url", null)));

        var error = await Assert.ThrowsAsync<AiSdkException>(() => CreateBatch(handler).DoGetResultsAsync(OperationCall(), CancellationToken.None));

        Assert.EndsWith("Anthropic batch \"msgbatch_123\" completed without batch output.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/anthropic/src/anthropic-batch.test.ts::Anthropic batch > batch result lifecycle::preserves tool items and continues with later results", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_tool_items_and_continues_with_later_results()
    {
        var toolCall = MessageWith(
            string.Empty,
            JsonNode.Parse("""[{"type":"tool_use","id":"toolu_123","name":"get_weather","input":{"city":"Paris"}}]""")!.AsArray(),
            ("stop_reason", "tool_use"));
        var handler = new RecordingHandler();
        handler.Respond(BatchResponse(("request_counts", Counts(0, 2, 0, 0, 0)))).RespondChunks(
            ResultsUrl,
            SucceededLine("tool-call", toolCall) + "\n",
            SucceededLine("valid", MessageResultBody("Paris")));

        var results = await (await CreateBatch(handler).DoGetResultsAsync(OperationCall(), CancellationToken.None)).ToListAsync().AsTask();

        Assert.Equal(2, results.Count);
        Assert.Equal("tool-call", results[0].Id);
        var call = Assert.Single(Succeeded(results[0]).Content);
        Assert.Equal(("tool-call", "toolu_123", "get_weather", """{"city":"Paris"}"""), (call.Type, call.ToolCallId, call.ToolName, call.Text));
        Assert.Equal("valid", results[1].Id);
        var text = Assert.Single(Succeeded(results[1]).Content);
        Assert.Equal(("text", "Paris"), (text.Type, text.Text));
    }
}
