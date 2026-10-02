// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests;

public sealed class XaiReasoningTests
{
    [Fact]
    [UpstreamTest("packages/xai/src/supports-reasoning-effort.test.ts::supportsReasoningEffort::should return true for %s", Coverage = UpstreamCoverage.Covered)]
    public void Supports_reasoning_effort_on_models_that_accept_it()
    {
        foreach (var modelId in new[] { "grok-4.3", "grok-latest", "grok-4.20-multi-agent", "grok-4.20-multi-agent-0309", "grok-3-mini" })
        {
            Assert.True(XaiReasoning.SupportsReasoningEffort(modelId));
        }
    }

    [Fact]
    [UpstreamTest("packages/xai/src/supports-reasoning-effort.test.ts::supportsReasoningEffort::should return false for %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_reasoning_effort_on_grok_4_20_reasoning_models()
    {
        foreach (var modelId in new[] { "grok-4.20-reasoning", "grok-4.20-non-reasoning", "grok-4.20-0309-reasoning", "grok-4.20-non-reasoning" })
        {
            Assert.False(XaiReasoning.SupportsReasoningEffort(modelId));
        }

        Assert.False(XaiReasoning.SupportsReasoningEffort("grok-4.20-0309-non-reasoning"));
    }

    [Fact]
    public async Task Language_model_reads_the_responses_payload()
    {
        var handler = new CaptureHandler();
        var provider = XaiProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler);
        var result = await provider.LanguageModel("grok-4").DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } },
            CancellationToken.None);
        Assert.Contains("api.x.ai", handler.Uri!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Contains("/responses", handler.Uri.AbsoluteUri, StringComparison.Ordinal);
        Assert.StartsWith("Bearer ", handler.RequestHeaders["Authorization"], StringComparison.Ordinal);
        Assert.Equal("ok", result.Text);
    }
}
