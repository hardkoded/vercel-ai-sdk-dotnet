// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>interrupted interactions</c>.</summary>
public sealed class InterruptedInteractionsTests
{
    private const string Prefix =
        "packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::interrupted interactions::";

    [Theory]
    [InlineData(null, FinishReason.Interrupted)]
    [InlineData("in_progress", FinishReason.Interrupted)]
    [InlineData("requires_action", FinishReason.ToolCalls)]
    [InlineData("completed", FinishReason.Stop)]
    [InlineData("cancelled", FinishReason.Other)]
    [InlineData("unknown_status", FinishReason.Other)]
    [UpstreamTest(Prefix + "handles the last status %s as %s", Coverage = UpstreamCoverage.Covered)]
    public void Handles_the_last_status_as(string? status, FinishReason unified)
    {
        var events = new List<string>
        {
            """{"event_type":"interaction.created","interaction":{"id":"i1","status":"in_progress"}}""",
            """{"event_type":"step.start","index":0,"step":{"type":"model_output"}}""",
            """{"event_type":"step.delta","index":0,"delta":{"type":"text","text":"Partial answer"}}""",
        };
        if (status != null)
        {
            events.Add(
                "{\"event_type\":\"interaction.status_update\",\"status\":\"" + status + "\"}"
            );
        }

        var parts = GoogleInteractionsStreamSupport.Run(events.ToArray());

        Assert.Contains(parts, part => part is TextDeltaStreamPart { Delta: "Partial answer" });
        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        var finish = Assert.IsType<FinishStreamPart>(
            Assert.Single(parts, part => part is FinishStreamPart)
        );
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(unified == FinishReason.Interrupted ? null : status, finish.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves an explicit completion event without a status",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_an_explicit_completion_event_without_a_status()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            """{"event_type":"interaction.completed","interaction":{"id":"i1"}}"""
        );

        var finish = Assert.IsType<FinishStreamPart>(
            Assert.Single(parts, part => part is FinishStreamPart)
        );
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }
}
