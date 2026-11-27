// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;

namespace Vercel.AI.Tests.Upstream.Alibaba.SupportsPreservedThinking;

/// <summary>Port of <c>supports-preserved-thinking.test.ts</c> &gt; <c>supportsPreservedThinking</c>.</summary>
public sealed class SupportsPreservedThinkingTests
{
    private const string Prefix =
        "packages/alibaba/src/supports-preserved-thinking.test.ts::supportsPreservedThinking::";

    [Fact]
    [UpstreamTest(
        Prefix + "should support models from Alibaba preserved-thinking documentation",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_support_models_from_Alibaba_preserved_thinking_documentation()
    {
        Assert.True(SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwen3.7-max"));
        Assert.True(SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwen3.8-max"));
        Assert.True(
            SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwen3.7-max-2026-06-08")
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should not support other models", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_support_other_models()
    {
        Assert.False(SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwen3-max"));
        Assert.False(SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwen-plus"));
        Assert.False(SupportsPreservedThinkingChecker.SupportsPreservedThinking("qwq-plus"));
    }
}
