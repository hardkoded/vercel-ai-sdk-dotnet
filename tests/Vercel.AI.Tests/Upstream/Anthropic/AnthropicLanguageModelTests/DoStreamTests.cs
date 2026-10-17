// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should stream compaction content blocks with provider metadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_compaction_content_blocks_with_provider_metadata()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "anthropic-compaction.1.chunks.txt")).Where(line => line.Trim().Length > 0);
        var parts = await StreamAsync(string.Concat(lines.Select(line => "data: " + line + "\n\n")));

        var compactionStart = Assert.Single(parts.OfType<TextStartStreamPart>(), part => part.Id == "0");
        JsonAssert.Equal(compactionStart.ProviderMetadata!.Value, "{\"anthropic\":{\"type\":\"compaction\"}}");

        var compactionText = string.Concat(parts.OfType<TextDeltaStreamPart>().Where(part => part.Id == "0").Select(part => part.Delta));
        Assert.NotEmpty(compactionText);
        Assert.Contains("## Summary of Conversation", compactionText);
        Assert.Contains("React vs Vue.js", compactionText);

        var regularStart = Assert.Single(parts.OfType<TextStartStreamPart>(), part => part.Id == "1");
        Assert.Null(regularStart.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should stream complete signed on-demand compaction blocks",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_complete_signed_on_demand_compaction_blocks()
    {
        var parts = await StreamAsync(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_compaction\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-opus-5\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":0,\"output_tokens\":0}}}\n\n" +
            "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"compaction\",\"content\":\"Summary of the conversation.\",\"signature\":\"compaction-signature\"}}\n\n" +
            "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n" +
            "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"compaction\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":0,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":120,\"output_tokens\":30}]}}\n\n" +
            "data: {\"type\":\"message_stop\"}\n\n");

        var start = Assert.Single(parts.OfType<TextStartStreamPart>());
        Assert.Equal("0", start.Id);
        JsonAssert.Equal(start.ProviderMetadata!.Value, "{\"anthropic\":{\"type\":\"compaction\",\"signature\":\"compaction-signature\"}}");
        var delta = Assert.Single(parts.OfType<TextDeltaStreamPart>());
        Assert.Equal("0", delta.Id);
        Assert.Equal("Summary of the conversation.", delta.Delta);
        Assert.Null(delta.ProviderMetadata);
        Assert.Equal("0", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Equal("compaction", finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::should handle compaction_delta with null content",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_compaction_delta_with_null_content()
    {
        var parts = await StreamAsync(
            "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_test\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-3-haiku-20240307\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":100,\"output_tokens\":0},\"content\":[],\"stop_reason\":null}}\n\n" +
            "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"compaction\",\"content\":null}}\n\n" +
            "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"compaction_delta\",\"content\":null}}\n\n" +
            "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"compaction_delta\",\"content\":\"Summary of conversation.\"}}\n\n" +
            "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n" +
            "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":10}}\n\n" +
            "data: {\"type\":\"message_stop\"}\n\n");

        // No parsing errors should occur, and the null content delta is skipped.
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var delta = Assert.Single(parts.OfType<TextDeltaStreamPart>(), part => part.Id == "0");
        Assert.Equal("Summary of conversation.", delta.Delta);
    }

    private static async Task<List<LanguageModelStreamPart>> StreamAsync(string sse)
    {
        var handler = new CaptureHandler { ContentType = "text/event-stream", ResponseBody = sse };
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in await AnthropicParity.Client(handler).LanguageModel("claude-opus-5").DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }

    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doStream::";

    private static readonly string[] Chunks =
    {
        "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_01KfpJoAEabmH2iHRRFjQMAG\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":null,\"stop_sequence\":null,\"usage\":{\"input_tokens\":17,\"output_tokens\":1}}}\n\n",
        "data: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n",
        "data: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello, World!\"}}\n\n",
        "data: {\"type\":\"content_block_stop\",\"index\":0}\n\n",
        "data: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":227}}\n\n",
        "data: {\"type\":\"message_stop\"}\n\n",
    };

    [Fact]
    [UpstreamTest(Prefix + "should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_raw_response_headers()
    {
        var result = await Stream(new SseStreamHandler(Chunks, new Dictionary<string, string> { ["test-header"] = "test-value" }));

        SseStreamHandler.AssertDefaultHeaders(result.Response!.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var result = await Stream(new SseStreamHandler(Chunks, new Dictionary<string, string> { ["test-header"] = "test-value" }));

        var expected = JsonNode.Parse("""
            {"model":"claude-3-haiku-20240307","max_tokens":4096,"messages":[{"role":"user","content":[{"type":"text","text":"Hello"}]}],"stream":true}
            """);
        Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(result.Request!.Body!.Value.GetRawText())));
    }

    private static async Task<Vercel.AI.Provider.LanguageModelStreamResult> Stream(SseStreamHandler handler)
    {
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }, handler);
        var result = await provider.LanguageModel("claude-3-haiku-20240307").DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None);
        await foreach (var _ in result)
        {
        }

        return result;
    }
}
