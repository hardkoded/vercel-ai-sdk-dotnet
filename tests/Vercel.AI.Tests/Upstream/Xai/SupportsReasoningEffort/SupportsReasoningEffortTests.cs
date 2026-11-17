// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.SupportsReasoningEffort;

/// <summary>Port of <c>supports-reasoning-effort.test.ts</c> &gt; <c>supportsReasoningEffort</c>.</summary>
public sealed class SupportsReasoningEffortTests
{
    private const string Prefix =
        "packages/xai/src/supports-reasoning-effort.test.ts::supportsReasoningEffort::";

    [Theory]
    [InlineData("grok-4.3")]
    [InlineData("grok-latest")]
    [InlineData("grok-4.20-multi-agent")]
    [InlineData("grok-4.20-multi-agent-0309")]
    [InlineData("grok-3-mini")]
    [UpstreamTest(Prefix + "should return true for %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_true_for(string modelId)
    {
        Assert.True(XaiReasoningEffort.SupportsReasoningEffort(modelId));
    }

    [Theory]
    [InlineData("grok-4.20-reasoning")]
    [InlineData("grok-4.20-non-reasoning")]
    [InlineData("grok-4.20-0309-reasoning")]
    [InlineData("grok-4.20-0309-non-reasoning")]
    [UpstreamTest(Prefix + "should return false for %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_false_for(string modelId)
    {
        Assert.False(XaiReasoningEffort.SupportsReasoningEffort(modelId));
    }

    private const string NonePrefix =
        "packages/xai/src/supports-reasoning-effort.test.ts::supportsNoneReasoningEffort::";

    [Theory]
    [InlineData("grok-4.3")]
    [InlineData("grok-latest")]
    [InlineData("grok-4-fast")]
    [InlineData("grok-4.20-multi-agent")]
    [InlineData("grok-4.20-multi-agent-0309")]
    [InlineData("grok-3-mini")]
    [UpstreamTest(
        NonePrefix + "should return true for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Supports_none_should_return_true_for(string modelId)
    {
        Assert.True(XaiReasoningEffort.SupportsNoneReasoningEffort(modelId));
    }

    [Theory]
    [InlineData("grok-4.5")]
    [InlineData("grok-4.6")]
    [InlineData("grok-4.7")]
    [InlineData("grok-4.7-fast")]
    [InlineData("grok-5")]
    [InlineData("grok-5.1")]
    [UpstreamTest(
        NonePrefix + "should return false for %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Supports_none_should_return_false_for(string modelId)
    {
        Assert.False(XaiReasoningEffort.SupportsNoneReasoningEffort(modelId));
    }
}
