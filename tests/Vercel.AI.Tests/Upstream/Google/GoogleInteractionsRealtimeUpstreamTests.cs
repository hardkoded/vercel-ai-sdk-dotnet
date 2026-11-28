// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Interactions finish reasons and input conversion.</summary>
public sealed class GoogleInteractionsRealtimeUpstreamTests
{
    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" without function_call to \"stop\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_completed_without_a_function_call_to_stop()
    {
        Assert.Equal(FinishReason.Stop, GoogleInteractionsFinish.Map("completed", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"completed\" with function_call to \"tool-calls\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_completed_with_a_function_call_to_tool_calls()
    {
        Assert.Equal(FinishReason.ToolCalls, GoogleInteractionsFinish.Map("completed", true));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"requires_action\" to \"tool-calls\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_requires_action_to_tool_calls()
    {
        Assert.Equal(
            FinishReason.ToolCalls,
            GoogleInteractionsFinish.Map("requires_action", false)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"failed\" to \"error\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_failed_to_error()
    {
        Assert.Equal(FinishReason.Error, GoogleInteractionsFinish.Map("failed", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"incomplete\" to \"length\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_incomplete_to_length()
    {
        Assert.Equal(FinishReason.Length, GoogleInteractionsFinish.Map("incomplete", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"cancelled\" to \"other\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_cancelled_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("cancelled", false));
    }

    [Fact]
    [UpstreamTest(
        "packages/google/src/interactions/map-google-interactions-finish-reason.test.ts::mapGoogleInteractionsFinishReason::maps \"in_progress\" / unknown to \"other\"",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_in_progress_and_unknown_statuses_to_other()
    {
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("in_progress", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map("no-such-status", false));
        Assert.Equal(FinishReason.Other, GoogleInteractionsFinish.Map(null, false));
    }
}
