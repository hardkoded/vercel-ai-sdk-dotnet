// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.ToolCallConcurrency;

/// <summary>Port of <c>tool-call-concurrency.test.ts</c> &gt; <c>prepareToolCallConcurrency</c>.</summary>
public sealed class PrepareToolCallConcurrencyTests
{
    // 1.5 and NaN from the upstream it.each cannot be stored in int?.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [UpstreamTest(
        "packages/ai/src/generate-text/tool-call-concurrency.test.ts::prepareToolCallConcurrency::should reject invalid concurrency %s",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream also rejects 1.5 and NaN. An int? cannot hold them, so the type enforces that check.")]
    public void Should_reject_invalid_concurrency(int value)
    {
        Assert.Throws<InvalidArgumentError>(() => ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(2)]
    [UpstreamTest("packages/ai/src/generate-text/tool-call-concurrency.test.ts::prepareToolCallConcurrency::should accept concurrency %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_accept_concurrency(int? concurrency)
    {
        Assert.Equal(concurrency, ToolCallConcurrencyScheduler.PrepareToolCallConcurrency(concurrency));
    }
}
