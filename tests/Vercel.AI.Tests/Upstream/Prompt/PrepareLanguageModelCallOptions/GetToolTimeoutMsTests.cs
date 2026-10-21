// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.Prompt.PrepareLanguageModelCallOptions;

/// <summary>Port of <c>prepare-language-model-call-options.test.ts</c> &gt; <c>timeout helpers (from request-options)</c> &gt; <c>getToolTimeoutMs</c>.</summary>
public sealed class GetToolTimeoutMsTests
{
    private const string Prefix = "packages/ai/src/prompt/prepare-language-model-call-options.test.ts::timeout helpers (from request-options) > getToolTimeoutMs::";

    [Fact]
    [UpstreamTest(Prefix + "should return undefined when timeout is undefined", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_when_timeout_is_undefined()
    {
        Assert.Null(TimeoutConfiguration.GetToolTimeoutMs(null, "testTool"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined when timeout is a number", Coverage = UpstreamCoverage.Covered, Note = "The number form is a TimeSpan, which converts to a configuration with the total timeout only.")]
    public void Should_return_undefined_when_timeout_is_a_number()
    {
        Assert.Null(TimeoutConfiguration.GetToolTimeoutMs(TimeSpan.FromMilliseconds(5000), "testTool"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined when toolMs is not set", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_when_toolMs_is_not_set()
    {
        Assert.Null(TimeoutConfiguration.GetToolTimeoutMs(new TimeoutConfiguration { TotalMs = 10000 }, "testTool"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return toolMs when set", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_toolMs_when_set()
    {
        Assert.Equal(3000, TimeoutConfiguration.GetToolTimeoutMs(new TimeoutConfiguration { ToolMs = 3000 }, "testTool"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return toolMs alongside other timeout values", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_toolMs_alongside_other_timeout_values()
    {
        Assert.Equal(5000, TimeoutConfiguration.GetToolTimeoutMs(new TimeoutConfiguration { TotalMs = 30000, StepMs = 10000, ToolMs = 5000 }, "testTool"));
    }
}
