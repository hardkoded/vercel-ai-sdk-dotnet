// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; basic generation</c>.</summary>
public sealed class DoGenerateBasicGenerationTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > basic generation::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body()
    {
        var (capture, _) = await Generate();

        Assert.True(JsonEquals("""
            {"input":[{"content":[{"text":"Hello","type":"input_text"}],"role":"user","type":"message"}],"model":"gemma-7b-it"}
            """, System.Text.Json.Nodes.JsonNode.Parse(capture.Requests[0].Body)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should produce correct content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_produce_correct_content()
    {
        var (_, result) = await Generate();

        Assert.Equal(2, result.Content.Count);
        var reasoning = Assert.IsType<GeneratedReasoning>(result.Content[0]);
        Assert.Equal("reasoning content", reasoning.Text);
        Assert.True(JsonEquals("""
            {"open-responses":{"itemId":"rs_3l1z5wpifxkwxhj459ya7","reasoningContent":[{"text":"reasoning content","type":"reasoning_text"}],"reasoningSummary":[]}}
            """, System.Text.Json.Nodes.JsonNode.Parse(reasoning.ProviderMetadata!.Value.GetRawText())));
        var text = Assert.IsType<GeneratedText>(result.Content[1]);
        Assert.Equal("text content", text.Text);
        Assert.True(JsonEquals("""{"open-responses":{"itemId":"msg_p1y190hl7hj1xyfqr1cir"}}""", System.Text.Json.Nodes.JsonNode.Parse(text.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage_correctly()
    {
        var (_, result) = await Generate();

        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Null(result.Usage.CacheWriteTokens);
        Assert.Equal(136, result.Usage.NoCacheInputTokens);
        Assert.Equal(136, result.Usage.InputTokens);
        Assert.Equal(2456, result.Usage.ReasoningTokens);
        Assert.Equal(1221, result.Usage.TextTokens);
        Assert.Equal(3677, result.Usage.OutputTokens);
        Assert.True(JsonEquals("""
            {"input_tokens":136,"input_tokens_details":{"cached_tokens":0},"output_tokens":3677,"output_tokens_details":{"reasoning_tokens":2456},"total_tokens":3813}
            """, System.Text.Json.Nodes.JsonNode.Parse(result.Usage.Raw!.Value.GetRawText())));
    }

    private static async Task<(UpstreamCapture Capture, LanguageModelGenerateResult Result)> Generate()
    {
        var capture = new UpstreamCapture { ResponseBody = Fixture("lmstudio-basic.1") };
        var result = await CreateModel(capture).DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);
        return (capture, result);
    }
}
