// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > reasoning::";
    private const string Note = "Asserts the reasoning parts (ids and provider metadata). The upstream inline snapshot also lists the stream-start, response-metadata, text and finish parts.";
    private const string Rs = "rs_6808709f6fcc8191ad2e2fdd784017b3";

    [Fact]
    [UpstreamTest(Prefix + "should handle reasoning with summary", Note = Note)]
    public async Task Handles_reasoning_with_summary()
    {
        var parts = await Run(
            "{\"reasoningEffort\":\"low\",\"reasoningSummary\":\"auto\"}",
            Added(0, Rs), SummaryAdded(Rs, 0), SummaryDelta(Rs, 0, "first"), SummaryDelta(Rs, 0, "part"), SummaryDone(Rs, 0),
            SummaryAdded(Rs, 1), SummaryDelta(Rs, 1, "second"), SummaryDone(Rs, 1), Done(0, Rs));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {Rs}:0 item={Rs} enc=null",
                $"reasoning-delta {Rs}:0 item={Rs} first",
                $"reasoning-delta {Rs}:0 item={Rs} part",
                $"reasoning-end {Rs}:0 item={Rs}",
                $"reasoning-start {Rs}:1 item={Rs} enc=null",
                $"reasoning-delta {Rs}:1 item={Rs} second",
                $"reasoning-end {Rs}:1 item={Rs} enc=null",
            },
            parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle reasoning with empty summary", Note = Note)]
    public async Task Handles_reasoning_with_empty_summary()
    {
        var parts = await Run("{\"reasoningEffort\":\"low\",\"reasoningSummary\":null}", Added(0, Rs), Done(0, Rs));

        Assert.Equal(
            new[] { $"reasoning-start {Rs}:0 item={Rs} enc=null", $"reasoning-end {Rs}:0 item={Rs} enc=null" },
            parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle encrypted content with summary", Note = Note)]
    public async Task Handles_encrypted_content_with_summary()
    {
        var parts = await Run(
            "{\"reasoningEffort\":\"low\",\"reasoningSummary\":\"auto\",\"include\":[\"reasoning.encrypted_content\"]}",
            Added(0, Rs, "encrypted_reasoning_data_abc123"), SummaryAdded(Rs, 0), SummaryDelta(Rs, 0, "first"), SummaryDone(Rs, 0),
            SummaryAdded(Rs, 1), SummaryDelta(Rs, 1, "second"), SummaryDone(Rs, 1), Done(0, Rs, "encrypted_reasoning_data_final_def456"));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {Rs}:0 item={Rs} enc=encrypted_reasoning_data_abc123",
                $"reasoning-delta {Rs}:0 item={Rs} first",
                $"reasoning-end {Rs}:0 item={Rs}",
                $"reasoning-start {Rs}:1 item={Rs} enc=encrypted_reasoning_data_abc123",
                $"reasoning-delta {Rs}:1 item={Rs} second",
                $"reasoning-end {Rs}:1 item={Rs} enc=encrypted_reasoning_data_final_def456",
            },
            parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle encrypted content with empty summary", Note = Note)]
    public async Task Handles_encrypted_content_with_empty_summary()
    {
        var parts = await Run(
            "{\"reasoningEffort\":\"low\",\"reasoningSummary\":null,\"include\":[\"reasoning.encrypted_content\"]}",
            Added(0, Rs, "encrypted_reasoning_data_abc123"), Done(0, Rs, "encrypted_reasoning_data_final_def456"));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {Rs}:0 item={Rs} enc=encrypted_reasoning_data_abc123",
                $"reasoning-end {Rs}:0 item={Rs} enc=encrypted_reasoning_data_final_def456",
            },
            parts);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple reasoning blocks", Note = Note)]
    public async Task Handles_multiple_reasoning_blocks()
    {
        const string first = "rs_first_6808709f6fcc8191ad2e2fdd784017b3";
        const string second = "rs_second_7908809g7gcc9291be3e3fee895028c4";
        var parts = await Run(
            "{\"reasoningEffort\":\"medium\",\"reasoningSummary\":\"auto\"}",
            Added(0, first), SummaryAdded(first, 0), SummaryDelta(first, 0, "a"), SummaryDone(first, 0),
            SummaryAdded(first, 1), SummaryDelta(first, 1, "b"), SummaryDone(first, 1), Done(0, first),
            Added(2, second), SummaryAdded(second, 0), SummaryDelta(second, 0, "c"), SummaryDone(second, 0), Done(2, second));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {first}:0 item={first} enc=null",
                $"reasoning-delta {first}:0 item={first} a",
                $"reasoning-end {first}:0 item={first}",
                $"reasoning-start {first}:1 item={first} enc=null",
                $"reasoning-delta {first}:1 item={first} b",
                $"reasoning-end {first}:1 item={first} enc=null",
                $"reasoning-start {second}:0 item={second} enc=null",
                $"reasoning-delta {second}:0 item={second} c",
                $"reasoning-end {second}:0 item={second} enc=null",
            },
            parts);
    }

    [Fact]
    public async Task Ends_a_summary_part_when_it_is_done_for_stored_responses()
    {
        var parts = await Run(
            "{\"store\":true}",
            Added(0, Rs, "encrypted"), SummaryAdded(Rs, 0), SummaryDelta(Rs, 0, "x"), SummaryDone(Rs, 0), Done(0, Rs, "encrypted"));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {Rs}:0 item={Rs} enc=encrypted",
                $"reasoning-delta {Rs}:0 item={Rs} x",
                $"reasoning-end {Rs}:0 item={Rs}",
            },
            parts);
    }

    [Fact]
    public async Task Correlates_rotated_item_ids_by_output_index()
    {
        var parts = await Run(
            "{}",
            Added(0, Rs), SummaryAdded("rotated", 0, 0), SummaryDelta("rotated", 0, "x", 0), SummaryDone("rotated", 0, 0), Done(0, "rotated"));

        Assert.Equal(
            new[]
            {
                $"reasoning-start {Rs}:0 item={Rs} enc=null",
                $"reasoning-delta {Rs}:0 item={Rs} x",
                $"reasoning-end {Rs}:0 item={Rs} enc=null",
            },
            parts);
    }

    private static JsonObject Added(int index, string id, string? encrypted = null) => Item("response.output_item.added", index, id, encrypted);

    private static JsonObject Done(int index, string id, string? encrypted = null) => Item("response.output_item.done", index, id, encrypted);

    private static JsonObject Item(string type, int index, string id, string? encrypted)
    {
        var item = new JsonObject { ["id"] = id, ["type"] = "reasoning" };
        if (encrypted != null)
        {
            item["encrypted_content"] = encrypted;
        }

        return new JsonObject { ["type"] = type, ["output_index"] = index, ["item"] = item };
    }

    private static JsonObject SummaryAdded(string id, int summaryIndex, int? outputIndex = null) =>
        Summary("response.reasoning_summary_part.added", id, summaryIndex, outputIndex);

    private static JsonObject SummaryDone(string id, int summaryIndex, int? outputIndex = null) =>
        Summary("response.reasoning_summary_part.done", id, summaryIndex, outputIndex);

    private static JsonObject SummaryDelta(string id, int summaryIndex, string delta, int? outputIndex = null)
    {
        var node = Summary("response.reasoning_summary_text.delta", id, summaryIndex, outputIndex);
        node["delta"] = delta;
        return node;
    }

    private static JsonObject Summary(string type, string id, int summaryIndex, int? outputIndex)
    {
        var node = new JsonObject { ["type"] = type, ["item_id"] = id, ["summary_index"] = summaryIndex };
        if (outputIndex != null)
        {
            node["output_index"] = outputIndex;
        }

        return node;
    }

    // Streams the events and describes the reasoning parts, one line per part.
    private static async Task<List<string>> Run(string openaiOptions, params JsonObject[] events)
    {
        var all = new List<JsonNode>
        {
            new JsonObject { ["type"] = "response.created", ["response"] = new JsonObject { ["id"] = "resp_1", ["model"] = "o3-mini", ["created_at"] = 1 } },
        };
        all.AddRange(events);
        all.Add(new JsonObject
        {
            ["type"] = "response.completed",
            ["response"] = new JsonObject { ["id"] = "resp_1", ["output"] = new JsonArray(), ["usage"] = new JsonObject { ["input_tokens"] = 1, ["output_tokens"] = 1 } },
        });

        var capture = new OpenAICapture { ServerSentEvents = OpenAIResponsesSupport.Sse(all) };
        var options = OpenAIUpstream.Hello();
        options.ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(openaiOptions);
        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("o3-mini").DoStreamAsync(options, CancellationToken.None);
        var parts = await OpenAIUpstream.Read(result);

        static string Describe(string type, string id, JsonElement? metadata, string? delta)
        {
            var openai = metadata!.Value.GetProperty("openai");
            var text = $"{type} {id} item={openai.GetProperty("itemId").GetString()}";
            if (delta != null)
            {
                return text + " " + delta;
            }

            return openai.TryGetProperty("reasoningEncryptedContent", out var encrypted)
                ? text + " enc=" + (encrypted.ValueKind == JsonValueKind.Null ? "null" : encrypted.GetString())
                : text;
        }

        return parts.Select(part => part switch
        {
            ReasoningStartStreamPart start => Describe("reasoning-start", start.Id, start.ProviderMetadata, null),
            ReasoningDeltaStreamPart delta => Describe("reasoning-delta", delta.Id, delta.ProviderMetadata, delta.Delta),
            ReasoningEndStreamPart end => Describe("reasoning-end", end.Id, end.ProviderMetadata, null),
            _ => null,
        }).OfType<string>().ToList();
    }
}
