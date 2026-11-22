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

    [Theory]
    [InlineData("none")]
    [InlineData("minimal")]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should map unsupported GPT-6 reasoning %s to the lowest supported effort",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_unsupported_GPT_6_reasoning_to_the_lowest_supported_effort(
        string reasoning
    )
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-6-astra",
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                Reasoning = reasoning,
            },
            false
        );

        Assert.Equal("gpt-6-astra", prepared.Body["model"]!.GetValue<string>());
        Assert.Equal("low", prepared.Body["reasoning"]!["effort"]!.GetValue<string>());
        AssertMappedWarning(prepared, reasoning, "low");
    }

    [Theory]
    [InlineData("gpt-5.5", "minimal", "low")]
    [InlineData("gpt-5.1", "xhigh", "high")]
    [InlineData("gpt-5-nano", "none", "minimal")]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should map unsupported reasoning for %s from %s to %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_unsupported_reasoning_for_model(
        string modelId,
        string reasoning,
        string expectedEffort
    )
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            modelId,
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                Reasoning = reasoning,
            },
            false
        );

        Assert.Equal(expectedEffort, prepared.Body["reasoning"]!["effort"]!.GetValue<string>());
        AssertMappedWarning(prepared, reasoning, expectedEffort);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should send max reasoning for GPT-5.6 models",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_max_reasoning_for_GPT_5_6_models()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-5.6-luna",
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                Reasoning = "max",
            },
            false
        );

        Assert.Equal("max", prepared.Body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }

    private static void AssertMappedWarning(
        OpenAIResponsesPreparedRequest prepared,
        string reasoning,
        string effort
    )
    {
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal("reasoning", warning.Feature);
        Assert.Equal(
            "reasoning \""
                + reasoning
                + "\" is not directly supported by this model. mapped to effort \""
                + effort
                + "\".",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should omit reasoning effort updates for models before GPT-6",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_reasoning_effort_updates_for_models_before_GPT_6()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-5.6",
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                    """{"reasoningEffortUpdate":"high"}"""
                ),
            },
            false
        );

        OpenAIUpstream.Equal(
            prepared.Body["input"],
            """[{ "role": "user", "content": [{ "type": "input_text", "text": "Hello" }] }]"""
        );
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("reasoningEffortUpdate", warning.Feature);
        Assert.Equal(
            "Configuration updates are only supported by GPT-6 and later models",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should omit reasoning effort updates with incompatible automatic context management",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_reasoning_effort_updates_with_incompatible_automatic_context_management()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-6-astra",
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                    """{"reasoningEffortUpdate":"high","contextManagement":[{"type":"compaction","compactThreshold":1000}]}"""
                ),
            },
            false
        );

        OpenAIUpstream.Equal(
            prepared.Body["input"],
            """[{ "role": "user", "content": [{ "type": "input_text", "text": "Hello" }] }]"""
        );
        var warning = Assert.Single(prepared.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("reasoningEffortUpdate", warning.Feature);
        Assert.Equal(
            "Configuration updates require standard reasoning mode without automatic compaction or automatic truncation",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should send a 12h promptCacheOptions ttl and prewarm",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_a_12h_promptCacheOptions_ttl_and_prewarm()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-6.1-sol",
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                    """{"promptCacheOptions":{"mode":"implicit","ttl":"12h","prewarm":true}}"""
                ),
            },
            false
        );

        OpenAIUpstream.Equal(
            prepared.Body,
            """
            {
              "model": "gpt-6.1-sol",
              "input": [{ "role": "user", "content": [{ "type": "input_text", "text": "Hello" }] }],
              "prompt_cache_options": { "mode": "implicit", "ttl": "12h", "prewarm": true }
            }
            """
        );
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should send a 12h ttl on a prompt cache breakpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_a_12h_ttl_on_a_prompt_cache_breakpoint()
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare(
            "gpt-6.1-sol",
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new UserModelMessage(
                        new UserContentPart[]
                        {
                            new TextContentPart("Stable prefix")
                            {
                                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                                    """{"promptCacheBreakpoint":{"mode":"explicit","ttl":"12h"}}"""
                                ),
                            },
                        }
                    ),
                },
            },
            false
        );

        var part = prepared.Body["input"]![0]!["content"]![0]!;
        Assert.Equal("input_text", part["type"]!.GetValue<string>());
        Assert.Equal("Stable prefix", part["text"]!.GetValue<string>());
        OpenAIUpstream.Equal(
            part["prompt_cache_breakpoint"],
            """{ "mode": "explicit", "ttl": "12h" }"""
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should reject an unsupported promptCacheOptions ttl",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_an_unsupported_promptCacheOptions_ttl()
    {
        var capture = new OpenAICapture { ResponseJson = OpenAIUpstream.ResponsesEmpty };
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            OpenAIUpstream
                .Provider(capture)
                .ResponsesModel("gpt-6.1-sol")
                .DoGenerateAsync(
                    new LanguageModelCallOptions
                    {
                        Prompt = OpenAIUpstream.Hello().Prompt,
                        ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                            """{"promptCacheOptions":{"ttl":"24h"}}"""
                        ),
                    },
                    CancellationToken.None
                )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > basic text response::should return empty content for a prewarm response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_empty_content_for_a_prewarm_response()
    {
        var capture = new OpenAICapture { ResponseJson = PrewarmResponse };
        var result = await OpenAIUpstream
            .Provider(capture)
            .ResponsesModel("gpt-6.1-sol")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = OpenAIUpstream.Hello().Prompt,
                    ProviderOptions = OpenAIUpstream.OpenAIOptionsJson(
                        """{"promptCacheOptions":{"ttl":"12h","prewarm":true}}"""
                    ),
                },
                CancellationToken.None
            );

        Assert.Empty(result.Content);
        Assert.Empty(result.Warnings);
        Assert.Equal(2048, result.Usage.InputTokens);
        Assert.Equal(0, result.Usage.NoCacheInputTokens);
        Assert.Equal(0, result.Usage.CacheReadTokens);
        Assert.Equal(2048, result.Usage.CacheWriteTokens);
        Assert.Equal(0, result.Usage.OutputTokens);
    }
}
