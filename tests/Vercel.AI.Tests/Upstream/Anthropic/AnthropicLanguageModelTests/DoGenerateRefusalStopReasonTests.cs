// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; refusal stop reason</c>.</summary>
public sealed class DoGenerateRefusalStopReasonTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > refusal stop reason::";

    [Fact]
    [UpstreamTest(
        Prefix + "should map a classifier refusal to content-filter and expose stop details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_classifier_refusal_to_content_filter_and_expose_stop_details()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-refusal");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5"
        );

        Assert.Equal("refusal", result.RawFinishReason);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("stopDetails"),
            """
            {
              "category": "cyber",
              "explanation": "This request triggered restrictions on violative cyber content and was blocked under Anthropic's Usage Policy.",
              "recommendedModel": "claude-fable-5",
              "type": "refusal"
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should map a refusal without stop details to content-filter and omit stop details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_a_refusal_without_stop_details_to_content_filter_and_omit_stop_details()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-refusal-no-details");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5"
        );

        Assert.Equal("refusal", result.RawFinishReason);
        Assert.Equal(FinishReason.ContentFilter, result.FinishReason);
        Assert.False(
            result
                .ProviderMetadata!.Value.GetProperty("anthropic")
                .TryGetProperty("stopDetails", out _)
        );
    }
}
