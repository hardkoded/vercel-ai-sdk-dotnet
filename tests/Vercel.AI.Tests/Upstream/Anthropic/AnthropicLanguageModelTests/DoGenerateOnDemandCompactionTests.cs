// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; on-demand compaction</c> (the text metadata test).</summary>
public sealed class DoGenerateOnDemandCompactionTests
{
    [Fact]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > on-demand compaction::should preserve the signature on compaction responses",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_the_signature_on_compaction_responses()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_compaction\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"compaction\",\"content\":\"Summary of the conversation.\",\"signature\":\"compaction-signature\"}],\"model\":\"claude-opus-5\",\"stop_reason\":\"compaction\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":0,\"output_tokens\":0,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":120,\"output_tokens\":30}]}}");

        var result = await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-5");

        var text = Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Summary of the conversation.", text.Text);
        JsonAssert.Equal(text.ProviderMetadata!.Value, "{\"anthropic\":{\"type\":\"compaction\",\"signature\":\"compaction-signature\"}}");
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("compaction", result.RawFinishReason);
    }
}
