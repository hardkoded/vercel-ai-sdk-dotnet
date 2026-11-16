// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>data part schemas</c>.</summary>
public sealed class DataPartSchemasTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    // upstream: z.object({ city: z.string() })
    private static readonly Dictionary<string, JsonNode> DataPartSchemas = new()
    {
        ["weather"] = JsonNode.Parse(
            """
            {"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}
            """
        )!,
    };

    [Fact]
    [UpstreamTest(
        Prefix + "data part schemas::should reject a data part that does not match its schema",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_reject_a_data_part_that_does_not_match_its_schema()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            harness.ProcessAsync(
                """
                [{"type":"start"},{"type":"data-weather","id":"w1","data":{"city":42}}]
                """,
                new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, _ => { })
                {
                    DataPartSchemas = DataPartSchemas,
                }
            )
        );

        Assert.Contains(
            "Type validation failed for message.parts[0].data (data-weather, id: \"w1\")",
            error.Message,
            StringComparison.Ordinal
        );
        Assert.Empty(harness.Parts);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "data part schemas::should accept a data part that matches its schema",
        Coverage = UpstreamCoverage.Covered,
        Commit = "8ba3940d3dd1f3cd3c4365bbaec505c6ef0eb1e3"
    )]
    public async Task Should_accept_a_data_part_that_matches_its_schema()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"data-weather","id":"w1","data":{"city":"Berlin"}}]
            """,
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, _ => { })
            {
                DataPartSchemas = DataPartSchemas,
            }
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [{"type":"data-weather","id":"w1","data":{"city":"Berlin"}}]
            """,
            harness.Parts
        );
    }
}
