// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenResponses.MapOpenResponsesFinishReason;

/// <summary>Port of <c>map-open-responses-finish-reason.test.ts</c> &gt; <c>mapOpenResponsesFinishReason</c>.</summary>
public sealed class MapOpenResponsesFinishReasonTests
{
    private const string Prefix = "packages/open-responses/src/responses/map-open-responses-finish-reason.test.ts::mapOpenResponsesFinishReason::";

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls when hasToolCalls is true and finishReason is undefined", Coverage = UpstreamCoverage.Covered, Note = "C# has one null value for both undefined and null.")]
    public void Should_return_tool_calls_when_hasToolCalls_is_true_and_finishReason_is_undefined()
    {
        Assert.Equal(FinishReason.ToolCalls, OpenResponsesFinishReason.Map(null, hasToolCalls: true));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls when hasToolCalls is true and finishReason is null", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_tool_calls_when_hasToolCalls_is_true_and_finishReason_is_null()
    {
        Assert.Equal(FinishReason.ToolCalls, OpenResponsesFinishReason.Map(null, hasToolCalls: true));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return stop when hasToolCalls is false and finishReason is undefined", Coverage = UpstreamCoverage.Covered, Note = "C# has one null value for both undefined and null.")]
    public void Should_return_stop_when_hasToolCalls_is_false_and_finishReason_is_undefined()
    {
        Assert.Equal(FinishReason.Stop, OpenResponsesFinishReason.Map(null, hasToolCalls: false));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return stop when hasToolCalls is false and finishReason is null", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_stop_when_hasToolCalls_is_false_and_finishReason_is_null()
    {
        Assert.Equal(FinishReason.Stop, OpenResponsesFinishReason.Map(null, hasToolCalls: false));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return length when finishReason is max_output_tokens", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_length_when_finishReason_is_max_output_tokens()
    {
        Assert.Equal(FinishReason.Length, OpenResponsesFinishReason.Map("max_output_tokens", hasToolCalls: false));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return content-filter when finishReason is content_filter", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_content_filter_when_finishReason_is_content_filter()
    {
        Assert.Equal(FinishReason.ContentFilter, OpenResponsesFinishReason.Map("content_filter", hasToolCalls: false));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-calls when hasToolCalls is true and finishReason is unknown", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_tool_calls_when_hasToolCalls_is_true_and_finishReason_is_unknown()
    {
        Assert.Equal(FinishReason.ToolCalls, OpenResponsesFinishReason.Map("completed", hasToolCalls: true));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return other when hasToolCalls is false and finishReason is unknown", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_other_when_hasToolCalls_is_false_and_finishReason_is_unknown()
    {
        Assert.Equal(FinishReason.Other, OpenResponsesFinishReason.Map("completed", hasToolCalls: false));
    }
}
