// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; basic text response</c>.</summary>
public sealed class DoGenerateBasicTextResponseTests
{
    private const string PrewarmResponse = """
        {
          "id": "resp_prewarm",
          "object": "response",
          "created_at": 1741257730,
          "status": "completed",
          "error": null,
          "incomplete_details": null,
          "model": "gpt-6.1-sol",
          "output": [],
          "usage": {
            "input_tokens": 2048,
            "input_tokens_details": {
              "cached_tokens": 0,
              "cache_write_tokens": 2048,
              "cache_write_tokens_details": { "ttl_30m_tokens": 0, "ttl_12h_tokens": 2048 }
            },
            "output_tokens": 0,
            "output_tokens_details": { "reasoning_tokens": 0 },
            "total_tokens": 2048
          }
        }
        """;

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should send a 12h promptCacheOptions ttl and prewarm",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_send_a_12h_promptCacheOptions_ttl_and_prewarm()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-6.1-sol", new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("""{"promptCacheOptions":{"mode":"implicit","ttl":"12h","prewarm":true}}"""),
        }, false);

        OpenAIUpstream.Equal(prepared.Body, """
            {
              "model": "gpt-6.1-sol",
              "input": [{ "role": "user", "content": [{ "type": "input_text", "text": "Hello" }] }],
              "prompt_cache_options": { "mode": "implicit", "ttl": "12h", "prewarm": true }
            }
            """);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should send a 12h ttl on a prompt cache breakpoint",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_send_a_12h_ttl_on_a_prompt_cache_breakpoint()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-6.1-sol", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("Stable prefix")
                    {
                        ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("""{"promptCacheBreakpoint":{"mode":"explicit","ttl":"12h"}}"""),
                    },
                }),
            },
        }, false);

        var part = prepared.Body["input"]![0]!["content"]![0]!;
        Assert.Equal("input_text", part["type"]!.GetValue<string>());
        Assert.Equal("Stable prefix", part["text"]!.GetValue<string>());
        OpenAIUpstream.Equal(part["prompt_cache_breakpoint"], """{ "mode": "explicit", "ttl": "12h" }""");
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should reject an unsupported promptCacheOptions ttl",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_an_unsupported_promptCacheOptions_ttl()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIUpstream.ResponsesEmpty };
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            OpenAIUpstream.Provider(capture).ResponsesModel("gpt-6.1-sol").DoGenerateAsync(new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("""{"promptCacheOptions":{"ttl":"24h"}}"""),
            }, CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should return empty content for a prewarm response",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_empty_content_for_a_prewarm_response()
    {
        var capture = new OpenAICapture { ResponseJson = PrewarmResponse };
        var result = await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-6.1-sol").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = OpenAIUpstream.Hello().Prompt,
            ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("""{"promptCacheOptions":{"ttl":"12h","prewarm":true}}"""),
        }, CancellationToken.None);

        Assert.Empty(result.Content);
        Assert.Empty(result.Warnings);
        Assert.Equal(2048, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.NoCacheInputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(2048, result.Usage.CacheWriteTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
    }
}
