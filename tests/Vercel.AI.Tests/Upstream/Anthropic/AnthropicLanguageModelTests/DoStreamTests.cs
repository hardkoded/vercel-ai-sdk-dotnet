// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
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
